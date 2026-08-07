using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    /// <summary>
    /// Entity Framework implementation of IFeedbackRepository
    /// </summary>
    public class EFFeedbackRepository : EFRepository<Feedback>, IFeedbackRepository
    {
        public EFFeedbackRepository(AppDbContext context) : base(context)
        {
        }

        public override IEnumerable<Feedback> GetAll()
        {
            return _dbSet.Include(f => f.Student).Include(f => f.Course).ToList();
        }

        public IEnumerable<Feedback> GetByStudent(string studentId)
        {
            return _dbSet.Include(f => f.Course).Where(f => f.StudentId == studentId).ToList();
        }

        public IEnumerable<Feedback> GetByCourse(string courseId)
        {
            return _dbSet.Include(f => f.Student).Where(f => f.CourseId == courseId).ToList();
        }

        public IEnumerable<Feedback> GetByFaculty(string facultyId)
        {
            return _dbSet.Include(f => f.Student).Include(f => f.Course).Where(f => f.FacultyId == facultyId).ToList();
        }

        public Feedback? GetByStudentAndCourse(string studentId, string courseId)
        {
            return _dbSet.Include(f => f.Student).Include(f => f.Course).FirstOrDefault(f => f.StudentId == studentId && f.CourseId == courseId);
        }

        public IEnumerable<Feedback> GetBySemester(string semester)
        {
            return _dbSet.Include(f => f.Student).Include(f => f.Course).Where(f => f.Semester == semester).ToList();
        }
    }
}
