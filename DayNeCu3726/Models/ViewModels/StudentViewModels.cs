using System.ComponentModel.DataAnnotations;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.ViewModels
{
    public class StudentViewModel
    {
        public string? Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@sims\.edu$", ErrorMessage = "Email must end with @sims.edu")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Student Code")]
        public string? StudentCode { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-18);

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = "Male";

        [Required]
        [Display(Name = "Program")]
        public string Program { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Enrollment Year")]
        public int EnrollmentYear { get; set; } = DateTime.Now.Year;

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Address")]
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Academic Status")]
        public AcademicStatus AcademicStatus { get; set; } = AcademicStatus.Active;

        [Display(Name = "GPA")]
        public double GPA { get; set; } = 0.0;
    }

    public class StudentListViewModel
    {
        public IEnumerable<StudentViewModel> Students { get; set; } = new List<StudentViewModel>();
        public string SearchQuery { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public string? FilterProgram { get; set; }
        public string? FilterDepartment { get; set; }

        // ── Pagination state ────────────────────────────────────────────────
        // Added so the list view renders one bounded page instead of every student in the database.
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        /// <summary>1-based index of the first row on the current page, for the "showing X–Y" label.</summary>
        public int FirstItemOnPage => TotalCount == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

        /// <summary>1-based index of the last row on the current page.</summary>
        public int LastItemOnPage => Math.Min(PageNumber * PageSize, TotalCount);
    }
}
