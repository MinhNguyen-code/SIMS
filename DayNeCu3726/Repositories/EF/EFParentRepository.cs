using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    /// <summary>
    /// Entity Framework implementation of IParentRepository
    /// </summary>
    public class EFParentRepository : EFRepository<Parent>, IParentRepository
    {
        public EFParentRepository(AppDbContext context) : base(context)
        {
        }

        public Parent? GetByParentCode(string parentCode)
        {
            return _dbSet.FirstOrDefault(p => p.ParentCode == parentCode);
        }

        public Parent? GetByEmail(string email)
        {
            var lowerEmail = email.ToLower();
            return _dbSet.FirstOrDefault(p => p.Email.ToLower() == lowerEmail);
        }

        public Parent? GetByStudentId(string studentId)
        {
            return _dbSet.Include(p => p.Student).FirstOrDefault(p => p.StudentId == studentId);
        }

        public IEnumerable<Parent> SearchByName(string name)
        {
            return _dbSet.Where(p => Microsoft.EntityFrameworkCore.EF.Functions.Like(p.FullName, $"%{name}%")).ToList();
        }

        public string GenerateParentCode()
        {
            int count = _dbSet.Count();
            return $"PH{count + 1:D5}";
        }
    }
}
