using System.ComponentModel.DataAnnotations;

namespace final_project_Core.DTO
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecidedByName { get; set; }
    }

    public class CreateReviewDto
    {
        [Required]
        public int StationId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 3)]
        public string Content { get; set; } = string.Empty;
    }
}
