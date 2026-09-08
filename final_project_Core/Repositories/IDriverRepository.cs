using final_project_Core.Entities;

namespace final_project_Core.Repositories
{
    public interface IDriverRepository : IRepository<Driver>
    {
        Task<Driver?> GetByEmailAsync(string email, CancellationToken ct);
    }
}
