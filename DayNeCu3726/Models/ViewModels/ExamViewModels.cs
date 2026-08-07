using System.ComponentModel.DataAnnotations;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.ViewModels
{
    public class ExamViewModel
    {
        public string ExamId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty; // Midterm, Final, Quiz
        public DateTime ExamDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        public string Semester { get; set; } = string.Empty;
        public ExamStatus Status { get; set; }
        public string StatusBadgeClass => Status switch
        {
            ExamStatus.Scheduled => "bg-info text-white",
            ExamStatus.InProgress => "bg-warning text-dark",
            ExamStatus.Completed => "bg-success text-white",
            ExamStatus.Cancelled => "bg-danger text-white",
            _ => "bg-secondary text-white"
        };
        public int EligibleStudentCount { get; set; }
    }

    public class CreateExamViewModel
    {
        [Required(ErrorMessage = "Please select a course")]
        [Display(Name = "Course")]
        public string CourseId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select the exam type")]
        [Display(Name = "Exam Type")]
        public string ExamType { get; set; } = "Final"; // Midterm, Final, Quiz

        [Required(ErrorMessage = "Please select the exam date")]
        [DataType(DataType.Date)]
        [Display(Name = "Exam Date")]
        public DateTime ExamDate { get; set; } = DateTime.Today.AddDays(14);

        [Required(ErrorMessage = "Please enter the time slot")]
        [Display(Name = "Time Slot")]
        public string TimeSlot { get; set; } = "09:00 - 11:00";

        [Required(ErrorMessage = "Please enter the room")]
        [Display(Name = "Room")]
        public string Room { get; set; } = "P.301";

        [Display(Name = "Supervisor (Faculty)")]
        public string? SupervisorId { get; set; }

        [Required]
        [Display(Name = "Semester")]
        public string Semester { get; set; } = "2025-1";
    }

    public class ExamDetailsViewModel
    {
        public ExamViewModel Exam { get; set; } = new();
        public IEnumerable<DayNeCu3726.Models.Entities.Student> EligibleStudents { get; set; } = new List<DayNeCu3726.Models.Entities.Student>();
    }
}
