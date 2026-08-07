using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Enrollment-specific repository interface.
    /// </summary>
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        IEnumerable<Enrollment> GetByStudent(string studentId);
        IEnumerable<Enrollment> GetByCourse(string courseId);
        Enrollment? GetByStudentAndCourse(string studentId, string courseId);
        IEnumerable<Enrollment> GetByStatus(EnrollmentStatus status);
        bool IsStudentEnrolled(string studentId, string courseId);
    }
}
