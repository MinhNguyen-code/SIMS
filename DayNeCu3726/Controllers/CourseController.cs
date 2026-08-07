using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Controllers
{
    /// <summary>
    /// Course management controller.
    /// Admin/Faculty: CRUD. Student: Read only.
    /// </summary>
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUnitOfWork _uow;

        public CourseController(ICourseService courseService, IUnitOfWork uow)
        {
            _courseService = courseService;
            _uow = uow;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetString("UserId") != null;
        private string GetRole() => HttpContext.Session.GetString("UserRole") ?? "";
        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        public IActionResult Index(string? search)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            IEnumerable<Course> courses;
            if (GetRole() == "Faculty")
                courses = _courseService.GetCoursesByFaculty(GetUserId());
            else
                courses = string.IsNullOrWhiteSpace(search)
                    ? _courseService.GetAllCourses()
                    : _courseService.SearchCourses(search);

            var vm = new CourseListViewModel
            {
                Courses = courses.Select(MapToViewModel),
                SearchQuery = search ?? "",
                TotalCount = _courseService.GetTotalCourses()
            };
            return View(vm);
        }

        public IActionResult Details(string id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var course = _courseService.GetCourseById(id);
            if (course == null) return NotFound();

            return View(MapToViewModel(course));
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            var vm = new CourseViewModel
            {
                Semester = "2024-2",
                MaxEnrollment = 40,
                Credits = 3
            };

            // Pre-fill faculty if current user is faculty
            if (GetRole() == "Faculty")
            {
                vm.FacultyId = GetUserId();
                vm.FacultyName = HttpContext.Session.GetString("UserName") ?? "";
            }

            ViewBag.Faculties = _uow.Users.GetByRole(Models.Enums.UserRole.Faculty)
                .Select(f => new { f.Id, f.FullName });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseViewModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Faculties = _uow.Users.GetByRole(Models.Enums.UserRole.Faculty)
                    .Select(f => new { f.Id, f.FullName });
                return View(model);
            }

            // Auto-fill faculty name from ID
            if (!string.IsNullOrEmpty(model.FacultyId))
            {
                var fac = _uow.Users.GetById(model.FacultyId);
                model.FacultyName = fac?.FullName ?? model.FacultyName;
            }

            var (success, message, course) = _courseService.CreateCourse(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Faculties = _uow.Users.GetByRole(Models.Enums.UserRole.Faculty)
                    .Select(f => new { f.Id, f.FullName });
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            var course = _courseService.GetCourseById(id);
            if (course == null) return NotFound();

            ViewBag.Faculties = _uow.Users.GetByRole(Models.Enums.UserRole.Faculty)
                .Select(f => new { f.Id, f.FullName });

            return View(MapToViewModel(course));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, CourseViewModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Faculties = _uow.Users.GetByRole(Models.Enums.UserRole.Faculty)
                    .Select(f => new { f.Id, f.FullName });
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.FacultyId))
            {
                var fac = _uow.Users.GetById(model.FacultyId);
                model.FacultyName = fac?.FullName ?? model.FacultyName;
            }

            var (success, message) = _courseService.UpdateCourse(id, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Faculties = _uow.Users.GetByRole(Models.Enums.UserRole.Faculty)
                    .Select(f => new { f.Id, f.FullName });
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            var course = _courseService.GetCourseById(id);
            if (course == null) return NotFound();
            return View(MapToViewModel(course));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            var (success, message) = _courseService.DeleteCourse(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }

        private static CourseViewModel MapToViewModel(Course c) => new()
        {
            CourseId = c.CourseId, CourseCode = c.CourseCode, Name = c.Name,
            Description = c.Description, Credits = c.Credits,
            FacultyId = c.FacultyId, FacultyName = c.FacultyName,
            MaxEnrollment = c.MaxEnrollment, CurrentEnrollment = c.CurrentEnrollment,
            Schedule = c.Schedule, Classroom = c.Classroom,
            Semester = c.Semester, Status = c.Status
        };
    }
}
