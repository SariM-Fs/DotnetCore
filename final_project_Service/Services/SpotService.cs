using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Enum;
using final_project_Core.Interface;
using final_project_Core.Repositories;

namespace final_project_Service.Services
{
    public class SpotService : ISpotService
    {
        private readonly ISpotRepository _spotRepository;
        private readonly IChargingSessionRepository _sessionRepository;

        public SpotService(ISpotRepository spotRepository, IChargingSessionRepository sessionRepository)
        {
            _spotRepository = spotRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<PagedResult<ChargingSpot>> GetPagedAsync(int? stationId, int page, int pageSize, CancellationToken ct)
        {
            var (items, totalCount) = await _spotRepository.GetPagedAsync(stationId, page, pageSize, ct);
            return new PagedResult<ChargingSpot>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ChargingSpot?> GetByIdAsync(int id, CancellationToken ct) => await _spotRepository.GetByIdReadOnlyAsync(id, ct);

        public async Task<Dictionary<int, int?>> GetActiveSessionIdsAsync(IEnumerable<int> spotIds, CancellationToken ct)
        {
            var sessions = await _sessionRepository.GetActiveSessionsForSpotsAsync(spotIds, ct);
            return sessions.ToDictionary(s => s.SpotId, s => (int?)s.Id);
        }

        public async Task<OperationResult<ChargingSpot>> SetStatusAsync(int spotId, SpotStatus status, CancellationToken ct)
        {
            var spot = await _spotRepository.GetByIdAsync(spotId, ct);
            if (spot is null)
            {
                return OperationResult<ChargingSpot>.NotFound($"Spot {spotId} not found.");
            }

            spot.Status = status;
            _spotRepository.Update(spot);
            await _spotRepository.SaveChangesAsync(ct);

            return OperationResult<ChargingSpot>.Success(spot);
        }
    }
}
