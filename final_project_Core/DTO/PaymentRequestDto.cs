using System.ComponentModel.DataAnnotations;
using final_project_Core.Enum;

namespace final_project_Core.DTO
{
    public class PaymentRequestDto : IValidatableObject
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SessionId must be a positive integer.")]
        public int SessionId { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [StringLength(19, MinimumLength = 8)]
        public string? CardNumber { get; set; }

        [StringLength(20)]
        public string? NationalId { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        // Cross-field shape validation: which fields are required depends on the
        // chosen Method. This only looks at the submitted DTO itself (not stored
        // data), so it belongs here and not in the Service layer.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Method == PaymentMethod.CreditCard && string.IsNullOrWhiteSpace(CardNumber))
            {
                yield return new ValidationResult(
                    "CardNumber is required when Method is CreditCard.",
                    new[] { nameof(CardNumber) });
            }

            if (Method == PaymentMethod.Bit && string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult(
                    "PhoneNumber is required when Method is Bit.",
                    new[] { nameof(PhoneNumber) });
            }
        }
    }

    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
