using final_project_Core.DTO;
using final_project_Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace final_project_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponseDto>> Pay([FromBody] PaymentRequestDto request, CancellationToken ct)
        {
            // Method validity and the CreditCard/Bit-specific required fields are
            // enforced by the enum model binder + PaymentRequestDto's IValidatableObject,
            // so [ApiController] already 400s bad input before this code runs.
            var payment = await _paymentService.PayAsync(
                request.SessionId, request.Method, request.CardNumber, request.NationalId, request.PhoneNumber, ct);

            var dto = new PaymentResponseDto { Id = payment.Id, Status = payment.Status, Message = "Payment approved." };
            return Created($"api/payments/{payment.Id}", dto);
        }
    }
}
