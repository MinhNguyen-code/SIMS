using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    public class EFEnrollmentRepository : EFRepository<Enrollment>, IEnrollmentRepository
    {
        public EFEnrollmentRepository(AppDbContext context) : base(context)
        {
        }

        public override Enrollment? GetById(string id)
        {
            return _dbSet.Include(e => e.Course)
                         .Include(e => e.Student)
                         .FirstOrDefault(e => e.EnrollmentId == id);
        }

        public override IEnumerable<Enrollment> GetAll()
        {
            return _dbSet.Include(e => e.Course)
                         .Include(e => e.Student)
                         .ToList();
        }

        public IEnumerable<Enrollment> GetByStudent(string studentId)
        {
            return _dbSet.Include(e => e.Course)
                         .Include(e => e.Student)
                         .Where(e => e.StudentId == studentId)
                         .ToList();
        }

        public IEnumerable<Enrollment> GetByCourse(string courseId)
        {
            return _dbSet.Include(e => e.Course)
                         .Include(e => e.Student)
                         .Where(e => e.CourseId == courseId)
                         .ToList();
        }

        public Enrollment? GetByStudentAndCourse(string studentId, string courseId)
        {
            return _dbSet.Include(e => e.Course)
                         .Include(e => e.Student)
                         .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public IEnumerable<Enrollment> GetByStatus(EnrollmentStatus status)
        {
            return _dbSet.Include(e => e.Course)
                         .Include(e => e.Student)
                         .Where(e => e.Status == status)
                         .ToList();
        }

        public bool IsStudentEnrolled(string studentId, string courseId)
        {
            return _dbSet.Any(e => e.StudentId == studentId && 
                                   e.CourseId == courseId && 
                                   e.Status == EnrollmentStatus.Enrolled);
        }
    }
}
