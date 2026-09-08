using final_project_Core.Common;
using final_project_Core.Entities;

namespace final_project_Core.Interface
{
    public interface IAmenityService
    {
        Task<IEnumerable<Amenity>> GetAllAsync(CancellationToken ct);
        Task<OperationResult<Amenity>> CreateAsync(string name, CancellationToken ct);
        Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken ct);
    }
}
