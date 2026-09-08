using final_project_Core.Enum;

namespace final_project_Core.Entities
{
    public class ChargingSpot
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public ChargingStation? Station { get; set; }

        public int SpotNumber { get; set; }
        public SpotStatus Status { get; set; } = SpotStatus.Available;

        // Concurrency token: EF Core will auto-increment this on every UPDATE.
        // If two requests read the same RowVersion and both try to update,
        // the second one will fail with DbUpdateConcurrencyException.
        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
