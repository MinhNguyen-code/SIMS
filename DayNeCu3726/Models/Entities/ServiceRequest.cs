using System;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Service request entity for administrative requests (sick leave, grade review, transcript, campus transfer).
    /// </summary>
    public class ServiceRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string StudentId { get; set; } = string.Empty;
        public RequestType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public string? AdminResponse { get; set; }
        public string? HandledByAdminId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // Navigation
        public Student? Student { get; set; }
    }
}
