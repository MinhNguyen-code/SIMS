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
    /// Entity Framework implementation of ITuitionRepository
    /// </summary>
    public class EFTuitionRepository : EFRepository<Tuition>, ITuitionRepository
    {
        public EFTuitionRepository(AppDbContext context) : base(context)
        {
        }

        public override Tuition? GetById(string id)
        {
            return _dbSet.Include(t => t.Student).Include(t => t.Payments).FirstOrDefault(t => t.TuitionId == id);
        }

        public IEnumerable<Tuition> GetByStudent(string studentId)
        {
            return _dbSet.Include(t => t.Payments).Where(t => t.StudentId == studentId).ToList();
        }

        public Tuition? GetByStudentAndSemester(string studentId, string semester)
        {
            return _dbSet.Include(t => t.Payments).FirstOrDefault(t => t.StudentId == studentId && t.Semester == semester);
        }

        public IEnumerable<Tuition> GetByStatus(TuitionStatus status)
        {
            return _dbSet.Include(t => t.Student).Where(t => t.Status == status).ToList();
        }

        public IEnumerable<Tuition> GetBySemester(string semester)
        {
            return _dbSet.Include(t => t.Student).Where(t => t.Semester == semester).ToList();
        }

        public IEnumerable<Tuition> GetOverdueTuitions()
        {
            return _dbSet.Include(t => t.Student)
                         .Where(t => t.Status != TuitionStatus.Paid && t.DueDate < DateTime.UtcNow)
                         .ToList();
        }
    }
}
