namespace final_project_Core.Entities
{
    public class ChargingSession
    {
        public int Id { get; set; }

        public int SpotId { get; set; }
        public ChargingSpot? Spot { get; set; }

        public int DriverId { get; set; }
        public Driver? Driver { get; set; } = null;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } // null while session is still active
        public double? EnergyDeliveredKwh { get; set; }

        public bool IsActive => EndTime == null;
    }
}
