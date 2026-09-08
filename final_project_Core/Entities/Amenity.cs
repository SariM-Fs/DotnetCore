namespace final_project_Core.Entities
{
    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<ChargingStation> Stations { get; set; } = new List<ChargingStation>();
    }
}
