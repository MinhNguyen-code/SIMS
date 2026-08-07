using System.ComponentModel.DataAnnotations;

namespace DayNeCu3726.Models.ViewModels
{
    public class FeedbackViewModel
    {
        public string FeedbackId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int TeachingQuality { get; set; }
        public int ContentRelevance { get; set; }
        public int Communication { get; set; }
        public int OverallRating { get; set; }
        public string? Comments { get; set; }
        public string Semester { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsEvaluated { get; set; }
    }

    public class CreateFeedbackViewModel
    {
        [Required]
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Please rate from 1 to 5 stars")]
        [Display(Name = "Teaching Quality")]
        public int TeachingQuality { get; set; } = 5;

        [Range(1, 5, ErrorMessage = "Please rate from 1 to 5 stars")]
        [Display(Name = "Content Relevance")]
        public int ContentRelevance { get; set; } = 5;

        [Range(1, 5, ErrorMessage = "Please rate from 1 to 5 stars")]
        [Display(Name = "Communication & Interaction")]
        public int Communication { get; set; } = 5;

        [Range(1, 5, ErrorMessage = "Please rate from 1 to 5 stars")]
        [Display(Name = "Overall Rating")]
        public int OverallRating { get; set; } = 5;

        [Display(Name = "Comments / Other Feedback")]
        public string? Comments { get; set; }

        public bool IsAnonymous { get; set; } = true;
    }
}
