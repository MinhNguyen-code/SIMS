using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Faculty entity. Inherits from User and adds faculty-specific fields.
    /// </summary>
    public class Faculty : User
    {
        public string FacultyCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;  // e.g., "Professor", "Lecturer"
        public string Specialization { get; set; } = string.Empty;

        // Navigation
        public List<Course> TeachingCourses { get; set; } = new();

        public Faculty()
        {
            Role = UserRole.Faculty;
        }
    }
}
