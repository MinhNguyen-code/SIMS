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
    /// Entity Framework implementation of IServiceRequestRepository
    /// </summary>
    public class EFServiceRequestRepository : EFRepository<ServiceRequest>, IServiceRequestRepository
    {
        public EFServiceRequestRepository(AppDbContext context) : base(context)
        {
        }

        public override ServiceRequest? GetById(string id)
        {
            return _dbSet.Include(sr => sr.Student).FirstOrDefault(sr => sr.RequestId == id);
        }

        public override IEnumerable<ServiceRequest> GetAll()
        {
            return _dbSet.Include(sr => sr.Student).ToList();
        }

        public IEnumerable<ServiceRequest> GetByStudent(string studentId)
        {
            return _dbSet.Include(sr => sr.Student).Where(sr => sr.StudentId == studentId).ToList();
        }

        public IEnumerable<ServiceRequest> GetByStatus(RequestStatus status)
        {
            return _dbSet.Include(sr => sr.Student).Where(sr => sr.Status == status).ToList();
        }

        public IEnumerable<ServiceRequest> GetByType(RequestType type)
        {
            return _dbSet.Include(sr => sr.Student).Where(sr => sr.Type == type).ToList();
        }

        public IEnumerable<ServiceRequest> GetPendingRequests()
        {
            return _dbSet.Include(sr => sr.Student)
                         .Where(sr => sr.Status == RequestStatus.Pending)
                         .OrderByDescending(sr => sr.CreatedAt)
                         .ToList();
        }
    }
}
