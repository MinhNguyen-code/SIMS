using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Enrollment entity – junction between Student and Course.
    /// Tracks enrollment status and grade.
    /// </summary>
    public class Enrollment
    {
        public string EnrollmentId { get; set; } = Guid.NewGuid().ToString();
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public DateTime EnrollDate { get; set; } = DateTime.UtcNow;
        public double? Grade { get; set; }           // Numeric 0–10
        public string? LetterGrade { get; set; }     // A/B/C/D/F (or P/M/D/F for BTEC)
        public int Absences { get; set; } = 0;       // Missed sessions (max allowed = 6)
        public int TotalSessions { get; set; } = 30; // Total sessions in BTEC is 30
        public string AttendancePattern { get; set; } = new string('_', 30); // 30 sessions representation
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
        public string? Remarks { get; set; }

        // Navigation (populated by service/repo)
        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }
}
