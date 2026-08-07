using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Patterns.Observer
{
    /// <summary>
    /// Concrete Observer: Audit log observer. Records all enrollment events
    /// to an in-memory audit trail for security and compliance purposes.
    /// </summary>
    public class AuditLogObserver : IEnrollmentObserver
    {
        private static readonly List<AuditEntry> _auditLog = new();

        public IReadOnlyList<AuditEntry> AuditLog => _auditLog.AsReadOnly();

        public void OnStudentEnrolled(Student student, Course course)
        {
            _auditLog.Add(new AuditEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "ENROLL",
                StudentId = student.Id,
                StudentName = student.FullName,
                CourseCode = course.CourseCode,
                CourseName = course.Name,
                Details = $"Student enrolled in {course.CourseCode}"
            });
        }

        public void OnStudentDropped(Student student, Course course)
        {
            _auditLog.Add(new AuditEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "DROP",
                StudentId = student.Id,
                StudentName = student.FullName,
                CourseCode = course.CourseCode,
                CourseName = course.Name,
                Details = $"Student dropped {course.CourseCode}"
            });
        }

        public void OnGradeUpdated(Student student, Course course, double grade)
        {
            _auditLog.Add(new AuditEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "GRADE_UPDATE",
                StudentId = student.Id,
                StudentName = student.FullName,
                CourseCode = course.CourseCode,
                CourseName = course.Name,
                Details = $"Grade updated to {grade:F1}"
            });
        }

        public static IReadOnlyList<AuditEntry> GetAllLogs() => _auditLog.AsReadOnly();
    }

    public class AuditEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
