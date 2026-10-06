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
            // PostgreSQL compares text case-sensitively (SQL Server didn't), so
            // compare lowercased to keep "ADMIN@gmail.com" == "admin@GMAIL.com".
            var normalized = email.ToLower();
            return await DbSet.FirstOrDefaultAsync(d => d.Email.ToLower() == normalized, ct);
        }
    }
}
