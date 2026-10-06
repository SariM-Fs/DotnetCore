using final_project_Core.Entities;

namespace final_project_Core.Repositories
{
    public interface ISpotRepository : IRepository<ChargingSpot>
    {
        // Tracked read: used on the mutate path (starting/ending a session) where
        // EF Core needs to track the entity so the Version (xmin) check applies on save.
        Task<ChargingSpot?> GetByIdWithStationAsync(int id, CancellationToken ct);

        // AsNoTracking read: used by pure GET endpoints that never save changes.
        Task<ChargingSpot?> GetByIdReadOnlyAsync(int id, CancellationToken ct);

        Task<List<ChargingSpot>> GetAllWithStationAsync(CancellationToken ct);
        Task<(List<ChargingSpot> Items, int TotalCount)> GetPagedAsync(int? stationId, int page, int pageSize, CancellationToken ct);
    }
}
