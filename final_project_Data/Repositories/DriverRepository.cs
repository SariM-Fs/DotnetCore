using final_project_Core.Entities;
using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace final_project_Data.Repositories
{
    public class DriverRepository : Repository<Driver>, IDriverRepository
    {
        public DriverRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Driver?> GetByEmailAsync(string email, CancellationToken ct)
        {
            return await DbSet.FirstOrDefaultAsync(d => d.Email == email, ct);
        }
    }
}
