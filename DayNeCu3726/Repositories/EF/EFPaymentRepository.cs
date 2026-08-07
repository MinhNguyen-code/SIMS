using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    /// <summary>
    /// Entity Framework implementation of IPaymentRepository
    /// </summary>
    public class EFPaymentRepository : EFRepository<Payment>, IPaymentRepository
    {
        public EFPaymentRepository(AppDbContext context) : base(context)
        {
        }

        public IEnumerable<Payment> GetByTuition(string tuitionId)
        {
            return _dbSet.Include(p => p.Tuition).Where(p => p.TuitionId == tuitionId).ToList();
        }
    }
}
