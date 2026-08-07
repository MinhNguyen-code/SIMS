using System;

namespace DayNeCu3726.Models.Entities
{
    public class AssignmentSubmission
    {
        public string SubmissionId { get; set; } = Guid.NewGuid().ToString();
        public string AssignmentId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // Store path to the PDF
        public string OriginalFileName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public double? Grade { get; set; }
        public string? Feedback { get; set; }

        // Navigation
        public Assignment? Assignment { get; set; }
        public Student? Student { get; set; }
    }
}
