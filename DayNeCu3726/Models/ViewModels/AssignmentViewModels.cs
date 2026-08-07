using System;
using System.Collections.Generic;

namespace DayNeCu3726.Models.ViewModels
{
    public class AssignmentViewModel
    {
        public string AssignmentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; }

        public int SubmissionCount { get; set; }
        public int GradedCount { get; set; }
        public bool HasSubmitted { get; set; }
        public double? MyGrade { get; set; }
    }

    public class CreateAssignmentViewModel
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(7);
    }

    public class SubmissionViewModel
    {
        public string SubmissionId { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public double? Grade { get; set; }
        public string? Feedback { get; set; }
    }
}
