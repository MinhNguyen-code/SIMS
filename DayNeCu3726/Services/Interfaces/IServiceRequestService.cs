using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using System.Collections.Generic;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IServiceRequestService
    {
        IEnumerable<ServiceRequestViewModel> GetRequestsByStudent(string studentId);
        IEnumerable<ServiceRequestViewModel> GetAllRequests();
        ServiceRequestViewModel? GetRequestById(string requestId);
        (bool success, string message) CreateRequest(string studentId, CreateServiceRequestViewModel model);
        (bool success, string message) UpdateStatus(string requestId, RequestStatus status, string adminResponse, string adminId);
    }
}
