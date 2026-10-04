namespace final_project_API.BackgroundServices
{
    public class SessionTimeoutOptions
    {
        public const string SectionName = "SessionTimeout";

        // A session still active this long after StartTime is auto-ended.
        public int MaxDurationMinutes { get; set; } = 720;

        // How often the sweep checks for expired sessions.
        public int CheckIntervalMinutes { get; set; } = 5;
    }
}
