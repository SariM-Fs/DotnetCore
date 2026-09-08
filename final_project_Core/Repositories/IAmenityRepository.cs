using final_project_Core.Entities;

namespace final_project_Core.Repositories
{
    public interface IAmenityRepository : IRepository<Amenity>
    {
        Task<List<Amenity>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}
