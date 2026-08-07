using System.Collections.Generic;
using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Payment entity
    /// </summary>
    public interface IPaymentRepository : IRepository<Payment>
    {
        IEnumerable<Payment> GetByTuition(string tuitionId);
    }
}
