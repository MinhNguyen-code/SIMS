using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.InMemory
{
    /// <summary>
    /// In-memory Enrollment repository implementation.
    /// </summary>
    public class EnrollmentRepository : InMemoryRepository<Enrollment>, IEnrollmentRepository
    {
        protected override string GetId(Enrollment entity) => entity.EnrollmentId;

        public IEnumerable<Enrollment> GetByStudent(string studentId)
            => _store.Where(e => e.StudentId == studentId);

        public IEnumerable<Enrollment> GetByCourse(string courseId)
            => _store.Where(e => e.CourseId == courseId);

        public Enrollment? GetByStudentAndCourse(string studentId, string courseId)
            => _store.FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);

        public IEnumerable<Enrollment> GetByStatus(EnrollmentStatus status)
            => _store.Where(e => e.Status == status);

        public bool IsStudentEnrolled(string studentId, string courseId)
            => _store.Any(e => e.StudentId == studentId
                            && e.CourseId == courseId
                            && e.Status == EnrollmentStatus.Enrolled);
    }
}
