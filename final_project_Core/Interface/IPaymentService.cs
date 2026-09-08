using final_project_Core.Entities;
using final_project_Core.Enum;

namespace final_project_Core.Interface
{
    public interface IPaymentService
    {
        Task<Payment> PayAsync(int sessionId, PaymentMethod method, string? cardNumber, string? nationalId, string? phoneNumber, CancellationToken ct);
    }
}
