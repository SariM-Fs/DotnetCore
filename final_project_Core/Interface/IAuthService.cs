using final_project_Core.Common;
using final_project_Core.Entities;

namespace final_project_Core.Interface
{
    public interface IAuthService
    {
        Task<OperationResult<(Driver Driver, string Token)>> RegisterAsync(
            string name, string email, string password, string licensePlate, CancellationToken ct);

        Task<OperationResult<(Driver Driver, string Token)>> LoginAsync(string email, string password, CancellationToken ct);
    }
}
