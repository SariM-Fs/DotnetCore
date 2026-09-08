using final_project_Core.Entities;
using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace final_project_Data.Repositories
{
    public class StationRepository : Repository<ChargingStation>, IStationRepository
    {
        public StationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ChargingStation>> GetAllWithDetailsAsync(CancellationToken ct)
        {
            return await DbSet
                .AsNoTracking()
                .Include(s => s.Spots)
                .Include(s => s.Amenities)
                .ToListAsync(ct);
        }
    }
}
