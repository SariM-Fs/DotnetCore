using final_project_Core.Common;
using final_project_Core.Entities;

namespace final_project_Core.Interface
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<Announcement>> GetAllAsync(CancellationToken ct);
        Task<OperationResult<Announcement>> CreateAsync(string title, string content, int createdByDriverId, string createdByName, CancellationToken ct);
        Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken ct);
    }
}
