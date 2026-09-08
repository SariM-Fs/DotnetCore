using System.ComponentModel.DataAnnotations;

namespace final_project_Core.DTO
{
    public class EndSessionRequestDto
    {
        // SessionId is NOT here: it comes from the route (PATCH /api/sessions/{id}).
        [Range(0, 1000, ErrorMessage = "EnergyDeliveredKwh must be between 0 and 1000.")]
        public double EnergyDeliveredKwh { get; set; }
    }
}
