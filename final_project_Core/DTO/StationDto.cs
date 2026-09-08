namespace final_project_Core.DTO
{
    public class StationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ConnectorType { get; set; } = string.Empty;
        public double PowerKw { get; set; }
        public bool IsActive { get; set; }
        public int TotalSpots { get; set; }
        public int AvailableSpots { get; set; }
        public List<string> Amenities { get; set; } = new();
    }
}
