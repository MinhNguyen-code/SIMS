using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Patterns.Observer
{
    /// <summary>
    /// Concrete Observer: Simulates email notification on enrollment events.
    /// In a real system, this would integrate with an email service (SMTP/SendGrid).
    /// </summary>
    public class EmailNotificationObserver : IEnrollmentObserver
    {
        private readonly List<string> _notifications = new();

        public IReadOnlyList<string> Notifications => _notifications.AsReadOnly();

        public void OnStudentEnrolled(Student student, Course course)
        {
            var msg = $"[EMAIL] {DateTime.Now:yyyy-MM-dd HH:mm} → " +
                      $"Dear {student.FullName}, you have been successfully enrolled in '{course.Name}' ({course.CourseCode}).";
            _notifications.Add(msg);
            Console.WriteLine(msg);
        }

        public void OnStudentDropped(Student student, Course course)
        {
            var msg = $"[EMAIL] {DateTime.Now:yyyy-MM-dd HH:mm} → " +
                      $"Dear {student.FullName}, you have dropped '{course.Name}' ({course.CourseCode}).";
            _notifications.Add(msg);
            Console.WriteLine(msg);
        }

        public void OnGradeUpdated(Student student, Course course, double grade)
        {
            var msg = $"[EMAIL] {DateTime.Now:yyyy-MM-dd HH:mm} → " +
                      $"Dear {student.FullName}, your grade for '{course.Name}' has been updated to {grade:F1}.";
            _notifications.Add(msg);
            Console.WriteLine(msg);
        }
    }
}
