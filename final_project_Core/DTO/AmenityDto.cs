using System.ComponentModel.DataAnnotations;

namespace final_project_Core.DTO
{
    public class AmenityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreateAmenityDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}
