using final_project_Core.Entities;

namespace final_project_Core.Repositories
{
    public interface IAnnouncementRepository : IRepository<Announcement>
    {
        Task<List<Announcement>> GetAllOrderedAsync(CancellationToken ct);
    }
}
