using System.ComponentModel.DataAnnotations;

namespace final_project_Core.DTO
{
    public class CreateStationDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string ConnectorType { get; set; } = string.Empty;

        [Range(0.1, 1000, ErrorMessage = "PowerKw must be between 0.1 and 1000.")]
        public double PowerKw { get; set; }

        public List<int> AmenityIds { get; set; } = new();
    }
}
