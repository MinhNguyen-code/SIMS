using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Student entity. Inherits from User and adds academic-specific fields.
    /// Demonstrates OOP Inheritance and Encapsulation.
    /// </summary>
    public class Student : User
    {
        public string StudentCode { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;  // e.g., "Computer Science"
        public string Department { get; set; } = string.Empty;
        public int EnrollmentYear { get; set; }
        public double GPA { get; set; } = 0.0;
        public AcademicStatus AcademicStatus { get; set; } = AcademicStatus.Active;
        public string? ProfileImageUrl { get; set; }

        // Navigation
        public List<Enrollment> Enrollments { get; set; } = new();

        public Student()
        {
            Role = UserRole.Student;
        }
    }
}
