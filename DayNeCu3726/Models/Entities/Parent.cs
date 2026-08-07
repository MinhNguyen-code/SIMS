using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Parent entity. Inherits from User. Linked to a Student for monitoring.
    /// </summary>
    public class Parent : User
    {
        public string ParentCode { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty; // Father, Mother, Guardian

        // Foreign key to Student
        public string? StudentId { get; set; }

        // Navigation
        public Student? Student { get; set; }

        public Parent()
        {
            Role = UserRole.Parent;
        }
    }
}
