using System.ComponentModel.DataAnnotations;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.ViewModels
{
    public class AnnouncementViewModel
    {
        public string AnnouncementId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? CourseId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public AnnouncementScope Scope { get; set; }
        public string ScopeName => Scope switch
        {
            AnnouncementScope.Course => "Course",
            AnnouncementScope.Department => "Department",
            AnnouncementScope.System => "System-wide",
            _ => "Announcement"
        };
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAnnouncementViewModel
    {
        [Required(ErrorMessage = "Please enter the announcement title")]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the announcement content")]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Scope")]
        public AnnouncementScope Scope { get; set; } = AnnouncementScope.System;

        [Display(Name = "Course (if Scope is Course)")]
        public string? CourseId { get; set; }

        [Display(Name = "Pin to top")]
        public bool IsPinned { get; set; }
    }
}
