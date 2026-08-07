using System;
using System.Collections.Generic;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Tuition record for a student per semester.
    /// Each enrolled course costs 4,500,000 VND.
    /// </summary>
    public class Tuition
    {
        public string TuitionId { get; set; } = Guid.NewGuid().ToString();
        public string StudentId { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty; // e.g., "2025-1"
        public int CourseCount { get; set; } = 0; // Number of courses enrolled
        public decimal CostPerCourse { get; set; } = 4_500_000m; // 4,500,000 VND per course
        public decimal TotalAmount { get; set; } = 0m; // CourseCount * CostPerCourse
        public decimal PaidAmount { get; set; } = 0m;
        public decimal RemainingAmount => TotalAmount - PaidAmount;
        public TuitionStatus Status { get; set; } = TuitionStatus.Unpaid;
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Student? Student { get; set; }
        public List<Payment> Payments { get; set; } = new();
    }
}
