using final_project_Core.Entities;
using final_project_Core.Enum;
using final_project_Core.Interface;
using final_project_Core.Repositories;

namespace final_project_Service.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Payment> PayAsync(int sessionId, PaymentMethod method, string? cardNumber, string? nationalId, string? phoneNumber, CancellationToken ct)
        {
            var payment = new Payment
            {
                SessionId = sessionId,
                Method = method.ToString(),
                Status = "Approved", // simulation - always succeeds
                CardLast4 = method == PaymentMethod.CreditCard && cardNumber?.Length >= 4 ? cardNumber[^4..] : null,
                NationalId = method == PaymentMethod.CreditCard ? nationalId : null,
                PhoneNumber = method == PaymentMethod.Bit ? phoneNumber : null,
            };

            await _paymentRepository.AddAsync(payment, ct);
            await _paymentRepository.SaveChangesAsync(ct);

            return payment;
        }
    }
}
