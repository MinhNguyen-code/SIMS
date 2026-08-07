using System;
using System.Collections.Generic;

namespace DayNeCu3726.Models.Entities
{
    public class Assignment
    {
        public string AssignmentId { get; set; } = Guid.NewGuid().ToString();
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Course? Course { get; set; }
        public List<AssignmentSubmission> Submissions { get; set; } = new();
    }
}
