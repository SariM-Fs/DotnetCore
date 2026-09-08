using final_project_Core.Entities;
using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace final_project_Data.Repositories
{
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {
        public AmenityRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Amenity>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            return await DbSet.Where(a => ids.Contains(a.Id)).ToListAsync(ct);
        }
    }
}
