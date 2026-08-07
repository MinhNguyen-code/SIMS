using DayNeCu3726.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DayNeCu3726.Models.ViewModels
{
    public class FacultyListViewModel
    {
        public IEnumerable<FacultyViewModel> Faculties { get; set; } = new List<FacultyViewModel>();
        public string SearchQuery { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class FacultyViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        public string FacultyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Position/Title")]
        public string Position { get; set; } = string.Empty;

        [Display(Name = "Specialization")]
        public string Specialization { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
        
        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;

        public List<CourseViewModel> TeachingCourses { get; set; } = new();
    }
}
