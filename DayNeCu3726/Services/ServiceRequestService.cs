using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayNeCu3726.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private static readonly List<ServiceRequestViewModel> _requests = new();

        public IEnumerable<ServiceRequestViewModel> GetRequestsByStudent(string studentId)
        {
            return _requests.Where(r => r.StudentId == studentId);
        }

        public IEnumerable<ServiceRequestViewModel> GetAllRequests()
        {
            return _requests;
        }

        public ServiceRequestViewModel? GetRequestById(string requestId)
        {
            return _requests.FirstOrDefault(r => r.RequestId == requestId);
        }

        public (bool success, string message) CreateRequest(string studentId, CreateServiceRequestViewModel model)
        {
            var newRequest = new ServiceRequestViewModel
            {
                RequestId = Guid.NewGuid().ToString(),
                StudentId = studentId,
                StudentName = "Student Name",
                StudentCode = "STU123",
                Type = model.Type,
                Title = model.Title,
                Description = model.Description,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.Now
            };
            
            _requests.Add(newRequest);
            return (true, "Request submitted successfully.");
        }

        public (bool success, string message) UpdateStatus(string requestId, RequestStatus status, string adminResponse, string adminId)
        {
            var request = _requests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null)
            {
                return (false, "Request not found.");
            }

            request.Status = status;
            request.AdminResponse = adminResponse;
            if (status == RequestStatus.Approved || status == RequestStatus.Rejected)
            {
                request.ResolvedAt = DateTime.Now;
            }

            return (true, "Status updated successfully.");
        }
    }
}
