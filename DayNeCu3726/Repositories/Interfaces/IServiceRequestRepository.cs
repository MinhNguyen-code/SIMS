using System.Collections.Generic;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for ServiceRequest entity
    /// </summary>
    public interface IServiceRequestRepository : IRepository<ServiceRequest>
    {
        IEnumerable<ServiceRequest> GetByStudent(string studentId);
        IEnumerable<ServiceRequest> GetByStatus(RequestStatus status);
        IEnumerable<ServiceRequest> GetByType(RequestType type);
        IEnumerable<ServiceRequest> GetPendingRequests();
    }
}
