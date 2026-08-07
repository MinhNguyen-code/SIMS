using System.Globalization;
using DayNeCu3726.DataProcessing.Abstractions;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.DataProcessing.Mapping
{
    /// <summary>
    /// Adapter pattern (structural) — adapts the flat, all-strings shape of a CSV record to the
    /// strongly typed <see cref="Student"/> domain entity, and back again for export.
    /// <para>
    /// Keeping the conversion here means the domain model never learns about CSV, and the CSV engine
    /// never learns about the domain. Either side can change independently, which is the Dependency
    /// Inversion Principle applied at the boundary of the system.
    /// </para>
    /// </summary>
    public sealed class StudentCsvMapper
    {
        private static readonly string[] DateFormats =
            { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd" };

        /// <summary>Column order used for both import templates and exports.</summary>
        public static IReadOnlyList<string> Columns { get; } = new[]
        {
            "StudentCode", "FullName", "Email", "DateOfBirth", "Gender",
            "Program", "Department", "EnrollmentYear", "GPA", "PhoneNumber",
            "Address", "AcademicStatus"
        };

        /// <summary>Columns that must be present for an import file to be accepted.</summary>
        public static IReadOnlyList<string> RequiredColumns { get; } = new[]
        {
            "FullName", "Email", "Program"
        };

        /// <summary>Builds a <see cref="Student"/> from one CSV record.</summary>
        public Student ToEntity(CsvRecord record, string passwordHash)
        {
            var student = new Student
            {
                StudentCode = record["StudentCode"],
                FullName = record["FullName"],
                Email = record["Email"].ToLowerInvariant(),
                PasswordHash = passwordHash,
                Gender = string.IsNullOrWhiteSpace(record["Gender"]) ? "Unspecified" : record["Gender"],
                Program = record["Program"],
                Department = string.IsNullOrWhiteSpace(record["Department"]) ? "General" : record["Department"],
                PhoneNumber = record["PhoneNumber"],
                Address = record["Address"],
                Role = UserRole.Student
            };

            student.DateOfBirth = ParseDate(record["DateOfBirth"]) ?? DateTime.UtcNow.AddYears(-18);
            student.EnrollmentYear = ParseInt(record["EnrollmentYear"]) ?? DateTime.UtcNow.Year;
            student.GPA = ParseDouble(record["GPA"]) ?? 0.0;
            student.AcademicStatus = ParseStatus(record["AcademicStatus"]);

            return student;
        }

        /// <summary>Flattens a <see cref="Student"/> into CSV field values in <see cref="Columns"/> order.</summary>
        public IReadOnlyList<string> ToRow(Student student) => new[]
        {
            student.StudentCode,
            student.FullName,
            student.Email,
            student.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            student.Gender,
            student.Program,
            student.Department,
            student.EnrollmentYear.ToString(CultureInfo.InvariantCulture),
            student.GPA.ToString("F2", CultureInfo.InvariantCulture),
            student.PhoneNumber,
            student.Address,
            student.AcademicStatus.ToString()
        };

        private static DateTime? ParseDate(string value) =>
            DateTime.TryParseExact(value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : null;

        private static int? ParseInt(string value) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

        private static double? ParseDouble(string value) =>
            double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

        private static AcademicStatus ParseStatus(string value) =>
            Enum.TryParse<AcademicStatus>(value, ignoreCase: true, out var parsed) ? parsed : AcademicStatus.Active;
    }
}
