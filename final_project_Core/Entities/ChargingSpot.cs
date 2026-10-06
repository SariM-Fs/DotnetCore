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

        // Concurrency token, mapped to PostgreSQL's xmin system column (see
        // ChargingSpotConfiguration). The database changes it on every UPDATE,
        // so if two requests read the same Version and both try to update,
        // the second one fails with DbUpdateConcurrencyException.
        public uint Version { get; set; }
    }
}
