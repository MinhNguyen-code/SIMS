using System.Text;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories;
using DayNeCu3726.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DayNeCu3726.Tests.TestDoubles
{
    /// <summary>
    /// Shared helpers for building test fixtures: isolated databases, sample entities and CSV streams.
    /// Centralising them keeps the individual test classes focused on the behaviour being verified.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// Creates a database context backed by a uniquely named in-memory store.
        /// The unique name guarantees test isolation — tests can run in parallel without
        /// one test's data leaking into another's assertions.
        /// </summary>
        public static AppDbContext CreateContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            return new AppDbContext(options);
        }

        /// <summary>Creates a Unit of Work over a fresh isolated in-memory database.</summary>
        public static IUnitOfWork CreateUnitOfWork(out AppDbContext context, string? databaseName = null)
        {
            context = CreateContext(databaseName);
            return new UnitOfWork(context);
        }

        public static Student CreateStudent(
            string fullName = "Nguyen Van A",
            string email = "nguyen.van.a@sims.edu",
            string program = "Computer Science",
            string studentCode = "BH00001",
            double gpa = 7.5,
            int enrollmentYear = 2024,
            AcademicStatus status = AcademicStatus.Active) => new()
            {
                FullName = fullName,
                Email = email,
                Program = program,
                Department = "Computing",
                StudentCode = studentCode,
                GPA = gpa,
                EnrollmentYear = enrollmentYear,
                AcademicStatus = status,
                DateOfBirth = new DateTime(2004, 5, 20),
                Gender = "Male",
                PasswordHash = FakePasswordHasher.Prefix + "Student@123",
                Role = UserRole.Student
            };

        public static Course CreateCourse(
            string courseCode = "CS101",
            string name = "Introduction to Programming",
            int credits = 3,
            int maxEnrollment = 40) => new()
            {
                CourseCode = courseCode,
                Name = name,
                Credits = credits,
                MaxEnrollment = maxEnrollment,
                Semester = "2025-1",
                Status = CourseStatus.Active
            };

        /// <summary>Turns CSV text into a readable stream, the input shape every importer expects.</summary>
        public static Stream CsvStream(string content) =>
            new MemoryStream(Encoding.UTF8.GetBytes(content));

        /// <summary>The canonical header used by the student import format.</summary>
        public const string StudentCsvHeader =
            "StudentCode,FullName,Email,DateOfBirth,Gender,Program,Department,EnrollmentYear,GPA,PhoneNumber,Address,AcademicStatus";

        /// <summary>Builds a valid student CSV containing <paramref name="rowCount"/> data rows.</summary>
        public static string BuildStudentCsv(int rowCount)
        {
            var builder = new StringBuilder();
            builder.AppendLine(StudentCsvHeader);

            for (var i = 1; i <= rowCount; i++)
            {
                builder.AppendLine(
                    $"BH{i:D5},Student {i},student{i}@sims.edu,2004-01-15,Male," +
                    $"Computer Science,Computing,2024,{(i % 10) + 0.5},0900000{i:D3},Hanoi,Active");
            }

            return builder.ToString();
        }
    }
}
