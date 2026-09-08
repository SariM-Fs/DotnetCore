using final_project_Core.Entities;
using final_project_Core.Enum;
using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace final_project_Data.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Review>> GetApprovedAsync(int? stationId, CancellationToken ct)
        {
            var query = DbSet.AsNoTracking()
                .Include(r => r.Station)
                .Where(r => r.Status == ReviewStatus.Approved);

            if (stationId.HasValue)
            {
                query = query.Where(r => r.StationId == stationId.Value);
            }

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        }

        public async Task<List<Review>> GetPendingAsync(CancellationToken ct)
        {
            return await DbSet.AsNoTracking()
                .Include(r => r.Station)
                .Where(r => r.Status == ReviewStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Review>> GetByDriverAsync(int driverId, CancellationToken ct)
        {
            return await DbSet.AsNoTracking()
                .Include(r => r.Station)
                .Where(r => r.DriverId == driverId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Review?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
        {
            return await DbSet
                .Include(r => r.Station)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }
    }
}
