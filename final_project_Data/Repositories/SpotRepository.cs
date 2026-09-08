using final_project_Core.Entities;
using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace final_project_Data.Repositories
{
    public class SpotRepository : Repository<ChargingSpot>, ISpotRepository
    {
        public SpotRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ChargingSpot?> GetByIdWithStationAsync(int id, CancellationToken ct)
        {
            return await DbSet
                .Include(s => s.Station)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<ChargingSpot?> GetByIdReadOnlyAsync(int id, CancellationToken ct)
        {
            return await DbSet
                .AsNoTracking()
                .Include(s => s.Station)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<List<ChargingSpot>> GetAllWithStationAsync(CancellationToken ct)
        {
            return await DbSet.AsNoTrackingWithIdentityResolution().Include(s => s.Station).ToListAsync(ct);
        }

        public async Task<(List<ChargingSpot> Items, int TotalCount)> GetPagedAsync(int? stationId, int page, int pageSize, CancellationToken ct)
        {
            var query = DbSet.AsNoTracking().Include(s => s.Station).AsQueryable();

            if (stationId.HasValue)
            {
                query = query.Where(s => s.StationId == stationId.Value);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(s => s.StationId).ThenBy(s => s.SpotNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
