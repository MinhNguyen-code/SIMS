namespace DayNeCu3726.Patterns.Singleton
{
    /// <summary>
    /// Singleton pattern – ensures only one instance of SystemConfiguration exists.
    /// Stores application-wide settings accessible from anywhere in the system.
    /// Thread-safe implementation using Lazy<T>.
    /// </summary>
    public sealed class SystemConfiguration
    {
        private static readonly Lazy<SystemConfiguration> _instance =
            new(() => new SystemConfiguration());

        private SystemConfiguration() { }

        public static SystemConfiguration Instance => _instance.Value;

        // Academic settings
        public string AcademicYear { get; set; } = "2024-2025";
        public string CurrentSemester { get; set; } = "2025-1";
        public int MaxCoursesPerStudent { get; set; } = 8;
        public int MinCreditsPerSemester { get; set; } = 12;
        public int MaxCreditsPerSemester { get; set; } = 24;

        // Grade settings
        public double MinPassingGrade { get; set; } = 5.0;
        public DayNeCu3726.Models.Enums.GradingScheme GradingScheme { get; set; } = DayNeCu3726.Models.Enums.GradingScheme.Btec;

        // System settings
        public string UniversityName { get; set; } = "SIMS University";
        public string SystemVersion { get; set; } = "1.0.0";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
