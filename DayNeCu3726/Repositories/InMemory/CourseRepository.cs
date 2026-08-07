using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.InMemory
{
    /// <summary>
    /// In-memory Course repository implementation.
    /// </summary>
    public class CourseRepository : InMemoryRepository<Course>, ICourseRepository
    {
        protected override string GetId(Course entity) => entity.CourseId;

        public Course? GetByCourseCode(string courseCode)
            => _store.FirstOrDefault(c => c.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Course> GetByFaculty(string facultyId)
            => _store.Where(c => c.FacultyId == facultyId);

        public IEnumerable<Course> GetByStatus(CourseStatus status)
            => _store.Where(c => c.Status == status);

        public IEnumerable<Course> GetBySemester(string semester)
            => _store.Where(c => c.Semester.Equals(semester, StringComparison.OrdinalIgnoreCase));

        private string GetTerm(string semester)
        {
            return semester switch {
                "Spring 2025" => "Term 1",
                "Summer 2025" => "Term 2",
                "Fall 2025" => "Term 3",
                "Spring 2026" => "Term 4",
                "Summer 2026" => "Term 5",
                "Fall 2026" => "Term 6",
                _ => "TBA"
            };
        }

        public IEnumerable<Course> SearchByName(string name)
            => _store.Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase)
                               || c.CourseCode.Contains(name, StringComparison.OrdinalIgnoreCase)
                               || c.Semester.Contains(name, StringComparison.OrdinalIgnoreCase)
                               || GetTerm(c.Semester).Contains(name, StringComparison.OrdinalIgnoreCase));
    }
}
