using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Enum;

namespace final_project_Core.Interface
{
    public interface ISpotService
    {
        Task<PagedResult<ChargingSpot>> GetPagedAsync(int? stationId, int page, int pageSize, CancellationToken ct);
        Task<ChargingSpot?> GetByIdAsync(int id, CancellationToken ct);
        Task<Dictionary<int, int?>> GetActiveSessionIdsAsync(IEnumerable<int> spotIds, CancellationToken ct);
        Task<OperationResult<ChargingSpot>> SetStatusAsync(int spotId, SpotStatus status, CancellationToken ct);
    }
}
