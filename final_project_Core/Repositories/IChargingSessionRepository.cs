using final_project_Core.Entities;

namespace final_project_Core.Repositories
{
    public interface IChargingSessionRepository : IRepository<ChargingSession>
    {
        Task<ChargingSession?> GetActiveSessionForSpotAsync(int spotId, CancellationToken ct);
        Task<List<ChargingSession>> GetActiveSessionsForSpotsAsync(IEnumerable<int> spotIds, CancellationToken ct);

        // Tracked (mutate path) so the timeout sweep can end them and free their spots.
        Task<List<ChargingSession>> GetActiveSessionsStartedBeforeAsync(DateTime cutoffUtc, CancellationToken ct);

        // Tracked read (mutate path for EndSessionAsync) that also eager-loads
        // Spot -> Station two levels deep via Include/ThenInclude, so the response
        // can show which station the session belongs to without a second query.
        Task<ChargingSession?> GetByIdWithDetailsAsync(int id, CancellationToken ct);

        // Same shape, AsNoTracking, for the pure-GET endpoint.
        Task<ChargingSession?> GetByIdWithDetailsReadOnlyAsync(int id, CancellationToken ct);
    }
}
