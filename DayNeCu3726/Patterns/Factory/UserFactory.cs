using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Patterns.Factory
{
    /// <summary>
    /// Factory Pattern – creates the appropriate User subclass (Student, Faculty, Admin)
    /// based on the requested role. Encapsulates object creation logic and reduces
    /// coupling between the client and concrete classes.
    /// </summary>
    public static class UserFactory
    {
        /// <summary>
        /// Creates a new User instance based on the specified role.
        /// </summary>
        public static User Create(UserRole role, string fullName, string email, string passwordHash)
        {
            return role switch
            {
                UserRole.Student => new Student
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = passwordHash,
                    EnrollmentYear = DateTime.Now.Year
                },
                UserRole.Faculty => new Faculty
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = passwordHash,
                    FacultyCode = $"FAC{DateTime.Now.Year}{new Random().Next(1000, 9999)}"
                },
                UserRole.Admin => new Admin
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = passwordHash,
                    AdminCode = $"ADM{DateTime.Now.Year}{new Random().Next(100, 999)}"
                },
                UserRole.Parent => new Parent
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = passwordHash,
                    ParentCode = $"PH{DateTime.Now.Year}{new Random().Next(1000, 9999)}"
                },
                _ => throw new ArgumentException($"Unknown role: {role}")
            };
        }

        /// <summary>
        /// Creates a Student with full details.
        /// </summary>
        public static Student CreateStudent(
            string fullName, string email, string passwordHash,
            string program, string department, DateTime dob, string gender)
        {
            return new Student
            {
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                Program = program,
                Department = department,
                DateOfBirth = dob,
                Gender = gender,
                EnrollmentYear = DateTime.Now.Year
            };
        }

        /// <summary>
        /// Creates a Faculty member with full details.
        /// </summary>
        public static Faculty CreateFaculty(
            string fullName, string email, string passwordHash,
            string department, string position, string specialization)
        {
            return new Faculty
            {
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                Department = department,
                Position = position,
                Specialization = specialization,
                FacultyCode = $"FAC{DateTime.Now.Year}{new Random().Next(1000, 9999)}"
            };
        }

        /// <summary>
        /// Creates a Parent with full details.
        /// </summary>
        public static Parent CreateParent(
            string fullName, string email, string passwordHash,
            string occupation, string relationship, string studentId)
        {
            return new Parent
            {
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                Occupation = occupation,
                Relationship = relationship,
                StudentId = studentId,
                ParentCode = $"PH{DateTime.Now.Year}{new Random().Next(1000, 9999)}"
            };
        }
    }
}
