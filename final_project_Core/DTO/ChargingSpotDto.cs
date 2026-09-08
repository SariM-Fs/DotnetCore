namespace final_project_Core.DTO
{
    public class ChargingSpotDto
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public int SpotNumber { get; set; }
        public int? ActiveSessionId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
