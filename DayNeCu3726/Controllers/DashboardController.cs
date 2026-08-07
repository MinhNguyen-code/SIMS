using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Patterns.Facade;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    /// <summary>
    /// Dashboard controller – shows role-specific statistics and activity feeds.
    /// Uses SIMSFacade to retrieve aggregate data.
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly SIMSFacade _facade;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IUnitOfWork _uow;

        public DashboardController(SIMSFacade facade, IStudentService studentService,
            ICourseService courseService, IEnrollmentService enrollmentService, IUnitOfWork uow)
        {
            _facade = facade;
            _studentService = studentService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _uow = uow;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("UserRole") ?? "Student";
            var vm = _facade.GetDashboardData(userId, role);

            vm.UserName = HttpContext.Session.GetString("UserName") ?? "";
            vm.UserEmail = HttpContext.Session.GetString("UserEmail") ?? "";
            vm.TotalFaculty = _uow.Users.GetByRole(UserRole.Faculty).Count();
            vm.ActiveCourses = _courseService.GetActiveCourses().Count();

            // Role-specific data
            if (role == "Student")
            {
                var student = _studentService.GetStudentById(userId);
                if (student != null)
                {
                    vm.MyGPA = student.GPA;
                    var myEnrollments = _enrollmentService.GetEnrollmentsByStudent(userId).ToList();
                    vm.MyCredits = myEnrollments
                        .Where(e => e.Status == EnrollmentStatus.Completed || e.Status == EnrollmentStatus.Enrolled)
                        .Sum(e => e.Course?.Credits ?? 0);
                    vm.MyEnrollments = myEnrollments.Select(e => new EnrollmentViewModel
                    {
                        EnrollmentId = e.EnrollmentId,
                        CourseName = e.Course?.Name ?? "",
                        CourseCode = e.Course?.CourseCode ?? "",
                        Credits = e.Course?.Credits ?? 0,
                        EnrollDate = e.EnrollDate,
                        Grade = e.Grade,
                        LetterGrade = e.LetterGrade,
                        Status = e.Status,
                        Schedule = e.Course?.Schedule ?? "",
                        FacultyName = e.Course?.FacultyName ?? ""
                    });
                }
            }
            else if (role == "Faculty")
            {
                var myCourses = _courseService.GetCoursesByFaculty(userId).ToList();
                vm.MyCourses = myCourses.Select(MapCourse);
                vm.MyStudents = myCourses.Sum(c => c.CurrentEnrollment);
            }
            else // Admin
            {
                vm.RecentStudents = _studentService.GetRecentStudents(5).Select(MapStudent);
                vm.RecentEnrollments = _enrollmentService.GetRecentEnrollments(5).Select(MapEnrollment);
                vm.PopularCourses = _courseService.GetAllCourses()
                    .OrderByDescending(c => c.CurrentEnrollment)
                    .Take(5)
                    .Select(MapCourse);
            }

            return View(vm);
        }

        private static StudentViewModel MapStudent(Student s) => new()
        {
            Id = s.Id, FullName = s.FullName, Email = s.Email,
            StudentCode = s.StudentCode, Program = s.Program,
            Department = s.Department, GPA = s.GPA, AcademicStatus = s.AcademicStatus
        };

        private static CourseViewModel MapCourse(Course c) => new()
        {
            CourseId = c.CourseId, CourseCode = c.CourseCode, Name = c.Name,
            Credits = c.Credits, FacultyName = c.FacultyName,
            MaxEnrollment = c.MaxEnrollment, CurrentEnrollment = c.CurrentEnrollment,
            Schedule = c.Schedule, Status = c.Status
        };

        private static EnrollmentViewModel MapEnrollment(Enrollment e) => new()
        {
            EnrollmentId = e.EnrollmentId, StudentId = e.StudentId,
            StudentName = e.Student?.FullName ?? "",
            CourseCode = e.Course?.CourseCode ?? "",
            CourseName = e.Course?.Name ?? "",
            EnrollDate = e.EnrollDate, Grade = e.Grade,
            LetterGrade = e.LetterGrade, Status = e.Status
        };
    }
}
