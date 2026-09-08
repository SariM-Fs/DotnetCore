using final_project_Core.Entities;
using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace final_project_Data.Repositories
{
    public class ChargingSessionRepository : Repository<ChargingSession>, IChargingSessionRepository
    {
        public ChargingSessionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ChargingSession?> GetActiveSessionForSpotAsync(int spotId, CancellationToken ct)
        {
            return await DbSet
                .Where(s => s.SpotId == spotId && s.EndTime == null)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<ChargingSession>> GetActiveSessionsForSpotsAsync(IEnumerable<int> spotIds, CancellationToken ct)
        {
            return await DbSet
                .AsNoTracking()
                .Where(s => spotIds.Contains(s.SpotId) && s.EndTime == null)
                .ToListAsync(ct);
        }

        public async Task<List<ChargingSession>> GetActiveSessionsStartedBeforeAsync(DateTime cutoffUtc, CancellationToken ct)
        {
            return await DbSet
                .Include(s => s.Spot)
                .Where(s => s.EndTime == null && s.StartTime < cutoffUtc)
                .ToListAsync(ct);
        }

        public async Task<ChargingSession?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
        {
            return await DbSet
                .Include(s => s.Spot)
                    .ThenInclude(sp => sp!.Station)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<ChargingSession?> GetByIdWithDetailsReadOnlyAsync(int id, CancellationToken ct)
        {
            return await DbSet
                .AsNoTracking()
                .Include(s => s.Spot)
                    .ThenInclude(sp => sp!.Station)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }
    }
}
