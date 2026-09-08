using System.ComponentModel.DataAnnotations;

namespace final_project_Core.DTO
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    public class CreateAnnouncementDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}
