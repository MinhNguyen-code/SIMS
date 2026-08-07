using System.ComponentModel.DataAnnotations;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.ViewModels
{
    public class EnrollmentViewModel
    {
        public string? EnrollmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int Credits { get; set; }
        public DateTime EnrollDate { get; set; }
        public double? Grade { get; set; }
        public string? LetterGrade { get; set; }
        public string? GradeDescription { get; set; }
        public int Absences { get; set; }
        public int TotalSessions { get; set; }
        public string AttendancePattern { get; set; } = string.Empty;
        public string DayPattern { get; set; } = string.Empty;
        public int SlotGroup { get; set; }
        public string Classroom { get; set; } = string.Empty;
        public EnrollmentStatus Status { get; set; }
        public string? Remarks { get; set; }
        public string FacultyName { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
    }

    public class GradeEntryViewModel
    {
        [Required]
        public string EnrollmentId { get; set; } = string.Empty;

        [Required]
        [Range(0, 10)]
        [Display(Name = "Grade (0-10)")]
        public double Grade { get; set; }

        [Required]
        [Range(0, 30, ErrorMessage = "Absences must be between 0 and 30.")]
        [Display(Name = "Absences (Max 30, Limit 6)")]
        public int Absences { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Display info
        public string StudentName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }

    public class CourseEnrollmentViewModel
    {
        public CourseViewModel Course { get; set; } = new();
        public IEnumerable<EnrollmentViewModel> Enrollments { get; set; } = new List<EnrollmentViewModel>();
        public bool IsAlreadyEnrolled { get; set; }
        public bool CanEnroll { get; set; }
    }
}
