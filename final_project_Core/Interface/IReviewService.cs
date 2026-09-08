using final_project_Core.Common;
using final_project_Core.Entities;

namespace final_project_Core.Interface
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetApprovedAsync(int? stationId, CancellationToken ct);
        Task<IEnumerable<Review>> GetPendingAsync(CancellationToken ct);
        Task<IEnumerable<Review>> GetMineAsync(int driverId, CancellationToken ct);

        Task<OperationResult<Review>> CreateAsync(int stationId, int driverId, string driverName, int rating, string content, CancellationToken ct);
        Task<OperationResult<Review>> ApproveAsync(int id, string adminName, CancellationToken ct);
        Task<OperationResult<Review>> RejectAsync(int id, string adminName, CancellationToken ct);
    }
}
