using System;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Exam schedule entity for managing midterm/final exams.
    /// </summary>
    public class Exam
    {
        public string ExamId { get; set; } = Guid.NewGuid().ToString();
        public string CourseId { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty; // Midterm, Final, Quiz
        public DateTime ExamDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty; // e.g., "09:00-11:00"
        public string Room { get; set; } = string.Empty;
        public string? SupervisorId { get; set; } // FK to Faculty
        public string? SupervisorName { get; set; }
        public string Semester { get; set; } = string.Empty;
        public ExamStatus Status { get; set; } = ExamStatus.Scheduled;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Course? Course { get; set; }
    }
}
