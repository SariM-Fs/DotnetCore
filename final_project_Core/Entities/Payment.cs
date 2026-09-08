namespace final_project_Core.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public ChargingSession? Session { get; set; }

        public string Method { get; set; } = string.Empty;   // "CreditCard" or "Bit"
        public string? CardLast4 { get; set; }                // only last 4 digits - never store the full PAN
        public string? NationalId { get; set; }
        public string? PhoneNumber { get; set; }
        public string Status { get; set; } = "Approved";       // this is a simulation, so it's always "Approved"
    }
}