using final_project_Core.Enum;

namespace final_project_Core.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public int StationId { get; set; }
        public ChargingStation? Station { get; set; }

        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public Driver? Driver { get; set; }

        public int Rating { get; set; } // 1-5
        public string Content { get; set; } = string.Empty;

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Set when an admin approves or rejects the review.
        public DateTime? DecidedAt { get; set; }
        public string? DecidedByName { get; set; }
    }
}
