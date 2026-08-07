using DayNeCu3726.Infrastructure.Authorization;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    [AuthorizeRole]
    public class FacultyController : Controller
    {
        private const int DefaultPageSize = 20;
        private readonly IFacultyService _facultyService;
        private readonly ICourseService _courseService;

        public FacultyController(IFacultyService facultyService, ICourseService courseService)
        {
            _facultyService = facultyService;
            _courseService = courseService;
        }

        private string GetRole() => HttpContext.Session.GetString("UserRole") ?? "";
        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        // --- Admin CRUD Actions ---

        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Index(string? search, int page = 1, int pageSize = DefaultPageSize)
        {
            pageSize = Math.Clamp(pageSize, 5, 100);
            var pagedFaculties = _facultyService.GetFacultiesPaged(page, pageSize, search);
            return View(pagedFaculties);
        }

        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Details(string id)
        {
            var faculty = _facultyService.GetFacultyById(id);
            if (faculty == null) return NotFound();

            var vm = new FacultyViewModel
            {
                Id = faculty.Id,
                FullName = faculty.FullName,
                Email = faculty.Email,
                FacultyCode = faculty.FacultyCode,
                Department = faculty.Department,
                Position = faculty.Position,
                Specialization = faculty.Specialization,
                PhoneNumber = faculty.PhoneNumber,
                Address = faculty.Address,
                IsActive = faculty.IsActive
            };
            
            // Get courses they teach
            var courses = _courseService.GetCoursesByFaculty(id);
            vm.TeachingCourses = courses.Select(c => new CourseViewModel
            {
                CourseId = c.CourseId,
                CourseCode = c.CourseCode,
                Name = c.Name,
                Credits = c.Credits,
                Schedule = c.Schedule,
                Semester = c.Semester,
                CurrentEnrollment = c.CurrentEnrollment,
                MaxEnrollment = c.MaxEnrollment
            }).ToList();

            return View(vm);
        }

        [HttpGet]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Create()
        {
            return View(new FacultyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Create(FacultyViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message, _) = _facultyService.CreateFaculty(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Edit(string id)
        {
            var faculty = _facultyService.GetFacultyById(id);
            if (faculty == null) return NotFound();

            return View(new FacultyViewModel
            {
                Id = faculty.Id,
                FullName = faculty.FullName,
                Email = faculty.Email,
                FacultyCode = faculty.FacultyCode,
                Department = faculty.Department,
                Position = faculty.Position,
                Specialization = faculty.Specialization,
                PhoneNumber = faculty.PhoneNumber,
                Address = faculty.Address,
                IsActive = faculty.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Edit(string id, FacultyViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = _facultyService.UpdateFaculty(id, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Delete(string id)
        {
            var faculty = _facultyService.GetFacultyById(id);
            if (faculty == null) return NotFound();

            return View(new FacultyViewModel
            {
                Id = faculty.Id,
                FullName = faculty.FullName,
                Email = faculty.Email,
                FacultyCode = faculty.FacultyCode,
                Department = faculty.Department
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult DeleteConfirmed(string id)
        {
            var (success, message) = _facultyService.DeleteFaculty(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // --- Faculty Teaching Actions ---

        [AuthorizeRole(UserRole.Faculty, UserRole.Admin)]
        public IActionResult Schedule()
        {
            var userId = GetUserId();
            var courses = _courseService.GetCoursesByFaculty(userId);

            var vm = courses.Select(c => new CourseViewModel
            {
                CourseId = c.CourseId,
                CourseCode = c.CourseCode,
                Name = c.Name,
                Credits = c.Credits,
                Schedule = c.Schedule,
                Classroom = c.Classroom,
                Semester = c.Semester,
                CurrentEnrollment = c.CurrentEnrollment,
                MaxEnrollment = c.MaxEnrollment
            });

            return View(vm);
        }

        [AuthorizeRole(UserRole.Faculty)]
        public IActionResult MyCourses()
        {
            var userId = GetUserId();
            var courses = _courseService.GetCoursesByFaculty(userId);

            var vm = courses.Select(c => new CourseViewModel
            {
                CourseId = c.CourseId,
                CourseCode = c.CourseCode,
                Name = c.Name,
                Credits = c.Credits,
                Schedule = c.Schedule,
                Classroom = c.Classroom,
                Semester = c.Semester,
                CurrentEnrollment = c.CurrentEnrollment,
                MaxEnrollment = c.MaxEnrollment,
                Status = c.Status
            });

            return View(vm);
        }
    }
}
