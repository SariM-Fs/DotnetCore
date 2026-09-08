namespace final_project_Core.Entities
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByDriverId { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public Driver? CreatedByDriver { get; set; }
    }
}
