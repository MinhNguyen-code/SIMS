using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Course-specific repository interface.
    /// </summary>
    public interface ICourseRepository : IRepository<Course>
    {
        Course? GetByCourseCode(string courseCode);
        IEnumerable<Course> GetByFaculty(string facultyId);
        IEnumerable<Course> GetByStatus(CourseStatus status);
        IEnumerable<Course> GetBySemester(string semester);
        IEnumerable<Course> SearchByName(string name);
    }
}
