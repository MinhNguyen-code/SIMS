namespace DayNeCu3726.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Stats
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalFaculty { get; set; }

        // Recent activity
        public IEnumerable<EnrollmentViewModel> RecentEnrollments { get; set; } = new List<EnrollmentViewModel>();
        public IEnumerable<StudentViewModel> RecentStudents { get; set; } = new List<StudentViewModel>();
        public IEnumerable<CourseViewModel> PopularCourses { get; set; } = new List<CourseViewModel>();

        // For student role
        public IEnumerable<EnrollmentViewModel> MyEnrollments { get; set; } = new List<EnrollmentViewModel>();
        public double MyGPA { get; set; }
        public int MyCredits { get; set; }

        // For faculty role
        public IEnumerable<CourseViewModel> MyCourses { get; set; } = new List<CourseViewModel>();
        public int MyStudents { get; set; }

        // Logged in user info
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
