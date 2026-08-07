using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Course entity representing a university course/subject.
    /// </summary>
    public class Course
    {
        public string CourseId { get; set; } = Guid.NewGuid().ToString();
        public string CourseCode { get; set; } = string.Empty;   // e.g., "CS101"
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? FacultyId { get; set; }
        public string FacultyName { get; set; } = string.Empty;
        public int MaxEnrollment { get; set; } = 40;
        public int CurrentEnrollment { get; set; } = 0;
        public string Schedule { get; set; } = string.Empty;     // e.g., "Mon/Wed 09:00–10:30"
        public string DayPattern { get; set; } = string.Empty;   // e.g., "Mon/Wed/Fri" or "Tue/Thu/Sat"
        public int SlotGroup { get; set; } = 1;                  // 1 = Slot 1-2, 2 = Slot 3-4, 3 = Slot 5-6
        public string Classroom { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;     // e.g., "2024-1"
        public CourseStatus Status { get; set; } = CourseStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<Enrollment> Enrollments { get; set; } = new();

        public bool HasCapacity => CurrentEnrollment < MaxEnrollment;
    }
}
