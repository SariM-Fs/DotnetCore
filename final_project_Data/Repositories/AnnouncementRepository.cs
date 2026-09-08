using final_project_Core.Entities;
using final_project_Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace final_project_Data.Repositories
{
    public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
    {
        public AnnouncementRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Announcement>> GetAllOrderedAsync(CancellationToken ct)
        {
            return await DbSet.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
        }
    }
}
