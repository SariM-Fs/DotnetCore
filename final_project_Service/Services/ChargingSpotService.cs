using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Enum;
using final_project_Core.Interface;
using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace final_project_Service.Services
{
    public class ChargingSpotService : IChargingSpotService
    {
        private readonly ISpotRepository _spotRepository;
        private readonly IChargingSessionRepository _sessionRepository;
        private readonly ILogger<ChargingSpotService> _logger;

        public ChargingSpotService(
            ISpotRepository spotRepository,
            IChargingSessionRepository sessionRepository,
            ILogger<ChargingSpotService> logger)
        {
            _spotRepository = spotRepository;
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<OperationResult<ChargingSession>> GetByIdAsync(int sessionId, CancellationToken ct)
        {
            var session = await _sessionRepository.GetByIdWithDetailsReadOnlyAsync(sessionId, ct);
            return session is null
                ? OperationResult<ChargingSession>.NotFound($"Session {sessionId} not found.")
                : OperationResult<ChargingSession>.Success(session);
        }

        public async Task<OperationResult<ChargingSession>> StartSessionAsync(int spotId, int driverId, CancellationToken ct)
        {
            var spot = await _spotRepository.GetByIdWithStationAsync(spotId, ct);
            if (spot is null)
            {
                _logger.LogWarning("StartSession failed: spot {SpotId} not found", spotId);
                return OperationResult<ChargingSession>.NotFound($"Spot {spotId} not found.");
            }
            if (spot.Station is not null && !spot.Station.IsActive)
            {
                return OperationResult<ChargingSession>.Conflict(
                    $"Station '{spot.Station.Name}' is currently inactive.");
            }

            if (spot.Status != SpotStatus.Available)
            {
                _logger.LogInformation(
                    "StartSession rejected: spot {SpotId} already {Status}", spotId, spot.Status);
                return OperationResult<ChargingSession>.Conflict(
                    $"Spot {spotId} is currently {spot.Status}.");
            }

            spot.Status = SpotStatus.Occupied;
            _spotRepository.Update(spot);

            var session = new ChargingSession
            {
                SpotId = spotId,
                DriverId = driverId,
                StartTime = DateTime.UtcNow
            };
            await _sessionRepository.AddAsync(session, ct);

            try
            {
                await _spotRepository.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Concurrency conflict starting session on spot {SpotId} for driver {DriverId}",
                    spotId, driverId);

                return OperationResult<ChargingSession>.Conflict(
                    "This spot was just taken by another driver. Please choose a different spot.");
            }

            _logger.LogInformation(
                "Session {SessionId} started on spot {SpotId} by driver {DriverId}",
                session.Id, spotId, driverId);

            session.Spot = spot;
            return OperationResult<ChargingSession>.Success(session);
        }

        public async Task<OperationResult<ChargingSession>> EndSessionAsync(int sessionId, int driverId, double energyDeliveredKwh, CancellationToken ct)
        {
            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, ct);

            if (session is null)
            {
                return OperationResult<ChargingSession>.NotFound($"Session {sessionId} not found.");
            }

            if (session.DriverId != driverId)
            {
                _logger.LogWarning(
                    "EndSession rejected: driver {DriverId} attempted to end session {SessionId} owned by driver {OwnerDriverId}",
                    driverId, sessionId, session.DriverId);
                return OperationResult<ChargingSession>.Forbidden(
                    "You can only end a charging session that you started.");
            }

            if (!session.IsActive)
            {
                return OperationResult<ChargingSession>.ValidationError(
                    $"Session {sessionId} has already ended.");
            }

            session.EndTime = DateTime.UtcNow;
            session.EnergyDeliveredKwh = energyDeliveredKwh;
            _sessionRepository.Update(session);

            if (session.Spot is not null)
            {
                session.Spot.Status = SpotStatus.Available;
                _spotRepository.Update(session.Spot);
            }

            try
            {
                await _sessionRepository.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Concurrency conflict ending session {SessionId}", sessionId);
                return OperationResult<ChargingSession>.Conflict(
                    "This session or spot was modified concurrently. Please retry.");
            }

            _logger.LogInformation("Session {SessionId} ended, {Energy}kWh delivered",
                sessionId, energyDeliveredKwh);

            return OperationResult<ChargingSession>.Success(session);
        }

        public async Task<int> EndExpiredSessionsAsync(TimeSpan maxDuration, CancellationToken ct)
        {
            var cutoffUtc = DateTime.UtcNow - maxDuration;
            var expiredSessions = await _sessionRepository.GetActiveSessionsStartedBeforeAsync(cutoffUtc, ct);

            if (expiredSessions.Count == 0)
            {
                return 0;
            }

            foreach (var session in expiredSessions)
            {
                session.EndTime = DateTime.UtcNow;
                _sessionRepository.Update(session);

                if (session.Spot is not null)
                {
                    session.Spot.Status = SpotStatus.Available;
                    _spotRepository.Update(session.Spot);
                }

                _logger.LogInformation(
                    "Session {SessionId} on spot {SpotId} auto-ended after exceeding the {MaxDuration} timeout",
                    session.Id, session.SpotId, maxDuration);
            }

            await _sessionRepository.SaveChangesAsync(ct);

            return expiredSessions.Count;
        }
    }
}
