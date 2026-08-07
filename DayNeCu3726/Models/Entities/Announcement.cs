using System;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Announcement entity for faculty/admin to send notifications to students.
    /// </summary>
    public class Announcement
    {
        public string AnnouncementId { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? CourseId { get; set; } // null = system-wide
        public AnnouncementScope Scope { get; set; } = AnnouncementScope.System;
        public bool IsPinned { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Course? Course { get; set; }
    }
}
