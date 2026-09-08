namespace final_project_Core.Entities
{
    public class ChargingStation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ConnectorType { get; set; } = string.Empty; // e.g. "Type2", "CCS"
        public double PowerKw { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ChargingSpot> Spots { get; set; } = new List<ChargingSpot>();
        public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    }
}
