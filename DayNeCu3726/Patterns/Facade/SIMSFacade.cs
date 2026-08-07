using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Patterns.Facade
{
    /// <summary>
    /// Facade Pattern – provides a simplified interface over the complex SIMS subsystems.
    /// Clients (e.g., external integrations, tests) can use SIMSFacade instead of
    /// juggling multiple services directly.
    /// </summary>
    public class SIMSFacade
    {
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IAuthService _authService;

        public SIMSFacade(
            IStudentService studentService,
            ICourseService courseService,
            IEnrollmentService enrollmentService,
            IAuthService authService)
        {
            _studentService = studentService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _authService = authService;
        }

        /// <summary>
        /// Register a new student and return their full profile.
        /// </summary>
        public (bool success, string message, Student? student) RegisterStudent(StudentViewModel model)
            => _studentService.CreateStudent(model);

        /// <summary>
        /// Enroll a student into a course by their IDs.
        /// </summary>
        public (bool success, string message) EnrollStudentInCourse(string studentId, string courseId)
            => _enrollmentService.EnrollStudent(studentId, courseId);

        /// <summary>
        /// Get a comprehensive dashboard data package.
        /// </summary>
        public DashboardViewModel GetDashboardData(string userId, string role)
        {
            var vm = new DashboardViewModel
            {
                TotalStudents = _studentService.GetTotalStudents(),
                TotalCourses = _courseService.GetTotalCourses(),
                TotalEnrollments = _enrollmentService.GetTotalEnrollments(),
                ActiveCourses = _courseService.GetActiveCourses().Count(),
                UserRole = role
            };
            return vm;
        }

        /// <summary>
        /// Authenticate a user by email and password.
        /// </summary>
        public User? AuthenticateUser(string email, string password)
            => _authService.Login(email, password);

        /// <summary>
        /// Get all active courses available for enrollment.
        /// </summary>
        public IEnumerable<Course> GetAvailableCourses()
            => _courseService.GetActiveCourses();

        /// <summary>
        /// Get a student's full academic transcript.
        /// </summary>
        public IEnumerable<Enrollment> GetStudentTranscript(string studentId)
            => _enrollmentService.GetEnrollmentsByStudent(studentId);
    }
}
