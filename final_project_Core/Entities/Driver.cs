namespace final_project_Core.Entities
{
    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // for JWT auth
        public string LicensePlate { get; set; } = string.Empty;
        public string Role { get; set; } = "User";

        public ICollection<ChargingSession> Sessions { get; set; } = new List<ChargingSession>();
    }
}
