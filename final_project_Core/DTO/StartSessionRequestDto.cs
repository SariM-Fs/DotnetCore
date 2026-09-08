using System.ComponentModel.DataAnnotations;

namespace final_project_Core.DTO
{
    public class StartSessionRequestDto
    {
        // DriverId is intentionally NOT here: the driver starting the session is
        // always the authenticated caller (read from the JWT), never a client-supplied value.
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SpotId must be a positive integer.")]
        public int SpotId { get; set; }
    }
}
