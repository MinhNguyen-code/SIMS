using System.ComponentModel.DataAnnotations;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.ViewModels
{
    public class ServiceRequestViewModel
    {
        public string RequestId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public RequestType Type { get; set; }
        public string TypeName => Type switch
        {
            RequestType.SickLeave => "Sick Leave",
            RequestType.GradeReview => "Grade Review",
            RequestType.TranscriptRequest => "Transcript Request",
            RequestType.CampusTransfer => "Campus/Class Transfer",
            _ => "Other Request"
        };
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public string StatusText => Status switch
        {
            RequestStatus.Pending => "Pending",
            RequestStatus.InReview => "In Review",
            RequestStatus.Approved => "Approved",
            RequestStatus.Rejected => "Rejected",
            _ => "Unknown"
        };
        public string StatusBadgeClass => Status switch
        {
            RequestStatus.Pending => "bg-warning text-dark",
            RequestStatus.InReview => "bg-info text-white",
            RequestStatus.Approved => "bg-success text-white",
            RequestStatus.Rejected => "bg-danger text-white",
            _ => "bg-secondary text-white"
        };
        public string? AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class CreateServiceRequestViewModel
    {
        [Required(ErrorMessage = "Please select a request type")]
        [Display(Name = "Request Type")]
        public RequestType Type { get; set; }

        [Required(ErrorMessage = "Please enter a title")]
        [StringLength(150)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter detailed content")]
        [Display(Name = "Request Content")]
        public string Description { get; set; } = string.Empty;
    }
}
