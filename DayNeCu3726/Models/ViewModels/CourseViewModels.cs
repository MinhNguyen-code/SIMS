using System.ComponentModel.DataAnnotations;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.ViewModels
{
    public class CourseViewModel
    {
        public string? CourseId { get; set; }

        [Required]
        [Display(Name = "Course Code")]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Course Name")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Credits")]
        public int Credits { get; set; } = 3;

        [Display(Name = "Assigned Faculty")]
        public string? FacultyId { get; set; }

        [Display(Name = "Faculty Name")]
        public string? FacultyName { get; set; }

        [Required]
        [Range(1, 500)]
        [Display(Name = "Max Enrollment")]
        public int MaxEnrollment { get; set; } = 40;

        [Display(Name = "Current Enrollment")]
        public int CurrentEnrollment { get; set; } = 0;

        [Display(Name = "Schedule")]
        public string? Schedule { get; set; }

        [Display(Name = "Classroom")]
        public string? Classroom { get; set; }

        [Display(Name = "Semester")]
        public string? Semester { get; set; }

        [Display(Name = "Status")]
        public CourseStatus Status { get; set; } = CourseStatus.Active;

        public bool HasCapacity => CurrentEnrollment < MaxEnrollment;
        public double FillPercentage => MaxEnrollment > 0 ? (double)CurrentEnrollment / MaxEnrollment * 100 : 0;
    }

    public class CourseListViewModel
    {
        public IEnumerable<CourseViewModel> Courses { get; set; } = new List<CourseViewModel>();
        public string SearchQuery { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public string? FilterStatus { get; set; }
        public string? FilterSemester { get; set; }
    }
}
