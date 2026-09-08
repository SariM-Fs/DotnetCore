using final_project_Core.Entities;

namespace final_project_Core.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<List<Review>> GetApprovedAsync(int? stationId, CancellationToken ct);
        Task<List<Review>> GetPendingAsync(CancellationToken ct);
        Task<List<Review>> GetByDriverAsync(int driverId, CancellationToken ct);
        Task<Review?> GetByIdWithDetailsAsync(int id, CancellationToken ct);
    }
}
