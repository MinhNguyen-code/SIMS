using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    /// <summary>
    /// Entity Framework implementation of IExamRepository
    /// </summary>
    public class EFExamRepository : EFRepository<Exam>, IExamRepository
    {
        public EFExamRepository(AppDbContext context) : base(context)
        {
        }

        public override IEnumerable<Exam> GetAll()
        {
            return _dbSet.Include(e => e.Course).ToList();
        }

        public IEnumerable<Exam> GetByCourse(string courseId)
        {
            return _dbSet.Include(e => e.Course).Where(e => e.CourseId == courseId).ToList();
        }

        public IEnumerable<Exam> GetBySemester(string semester)
        {
            return _dbSet.Include(e => e.Course).Where(e => e.Semester == semester).ToList();
        }

        public IEnumerable<Exam> GetByStatus(ExamStatus status)
        {
            return _dbSet.Include(e => e.Course).Where(e => e.Status == status).ToList();
        }

        public IEnumerable<Exam> GetBySupervisor(string facultyId)
        {
            return _dbSet.Include(e => e.Course).Where(e => e.SupervisorId == facultyId).ToList();
        }

        public IEnumerable<Exam> GetByDate(DateTime date)
        {
            return _dbSet.Include(e => e.Course).Where(e => e.ExamDate.Date == date.Date).ToList();
        }
    }
}
