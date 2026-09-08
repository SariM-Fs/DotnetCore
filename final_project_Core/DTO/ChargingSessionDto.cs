namespace final_project_Core.DTO
{
    public class ChargingSessionDto
    {
        public int Id { get; set; }
        public int SpotId { get; set; }
        public int DriverId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? EnergyDeliveredKwh { get; set; }
        public bool IsActive { get; set; }
    }
}
