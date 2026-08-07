using System;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Student feedback/evaluation for faculty teaching quality at end of semester.
    /// </summary>
    public class Feedback
    {
        public string FeedbackId { get; set; } = Guid.NewGuid().ToString();
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string FacultyId { get; set; } = string.Empty;
        public int TeachingQuality { get; set; } // 1-5 stars
        public int ContentRelevance { get; set; } // 1-5 stars
        public int Communication { get; set; } // 1-5 stars
        public int OverallRating { get; set; } // 1-5 stars
        public string? Comments { get; set; }
        public string Semester { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public bool IsAnonymous { get; set; } = true;

        // Navigation
        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }
}
