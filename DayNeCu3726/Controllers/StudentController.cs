using DayNeCu3726.Infrastructure.Authorization;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    /// <summary>
    /// Student management controller. Admin: full CRUD. Faculty: read. Student: own profile only.
    /// <para>
    /// <b>Refactored.</b> Every action previously opened with the same hand-written guard clauses:
    /// <code>
    /// if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
    /// if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");
    /// </code>
    /// Those lines were duplicated eleven times in this file alone and once more in every other
    /// controller. The rule is now declared with <see cref="AuthorizeRoleAttribute"/>, which removes
    /// the duplication, makes each action's access policy readable at a glance, and — most
    /// importantly — means a newly added action cannot accidentally ship without a check.
    /// </para>
    /// <para>
    /// The class-level attribute establishes the baseline "must be signed in"; individual actions
    /// tighten it where a stricter role is required.
    /// </para>
    /// </summary>
    [AuthorizeRole]
    public class StudentController : Controller
    {
        /// <summary>Rows shown per page. Bounded so the page size cannot grow with the dataset.</summary>
        private const int DefaultPageSize = 20;

        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        }

        private string GetRole() => HttpContext.Session.GetString("UserRole") ?? "";
        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        /// <summary>
        /// Lists students one page at a time.
        /// <para>
        /// Previously this called <c>GetAllStudents()</c> (or <c>SearchStudents()</c>) and rendered
        /// every matching row. With a large dataset that meant loading the entire student table into
        /// memory and building an enormous HTML page on every visit. Paging keeps both the query and
        /// the rendered page bounded, satisfying the Performance and Scalability requirements.
        /// </para>
        /// </summary>
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Index(string? search, int page = 1, int pageSize = DefaultPageSize)
        {
            // Clamp the page size so a crafted query string cannot request the whole table.
            pageSize = Math.Clamp(pageSize, 5, 100);

            var pagedStudents = _studentService.GetStudentsPaged(page, pageSize, search);

            var viewModel = new StudentListViewModel
            {
                Students = pagedStudents.Items.Select(MapToViewModel),
                SearchQuery = search ?? "",
                TotalCount = pagedStudents.TotalCount,
                PageNumber = pagedStudents.PageNumber,
                PageSize = pagedStudents.PageSize,
                TotalPages = pagedStudents.TotalPages,
                HasPreviousPage = pagedStudents.HasPreviousPage,
                HasNextPage = pagedStudents.HasNextPage
            };

            return View(viewModel);
        }

        public IActionResult Details(string id)
        {
            // A student may only view their own record; this is a data-ownership rule rather than a
            // role rule, so it stays in the action where the record identifier is known.
            if (GetRole() == nameof(UserRole.Student) && id != GetUserId())
                return RedirectToAction("Details", new { id = GetUserId() });

            var student = _studentService.GetStudentById(id);
            if (student == null) return NotFound();

            return View(MapToViewModel(student));
        }

        [HttpGet]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Create() =>
            View(new StudentViewModel { EnrollmentYear = DateTime.Now.Year });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Create(StudentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message, _) = _studentService.CreateStudent(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AuthorizeRole(UserRole.Admin, UserRole.Student)]
        public IActionResult Edit(string id)
        {
            // A student may edit only their own record.
            if (GetRole() == nameof(UserRole.Student) && id != GetUserId())
                return RedirectToAction("AccessDenied", "Auth");

            var student = _studentService.GetStudentById(id);
            if (student == null) return NotFound();

            return View(MapToViewModel(student));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(UserRole.Admin, UserRole.Student)]
        public IActionResult Edit(string id, StudentViewModel model)
        {
            // Security fix: the POST handler previously performed no ownership check at all, so any
            // signed-in student could edit another student's record by changing the posted id.
            if (GetRole() == nameof(UserRole.Student) && id != GetUserId())
                return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid) return View(model);

            var (success, message) = _studentService.UpdateStudent(id, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(UserRole.Admin, UserRole.Faculty)]
        public IActionResult UpdateStatus(string id, AcademicStatus status)
        {
            var (success, message) = _studentService.UpdateStudentStatus(id, status);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Delete(string id)
        {
            var student = _studentService.GetStudentById(id);
            if (student == null) return NotFound();

            return View(MapToViewModel(student));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult DeleteConfirmed(string id)
        {
            var (success, message) = _studentService.DeleteStudent(id);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction(nameof(Index));
        }

        private static StudentViewModel MapToViewModel(Student s) => new()
        {
            Id = s.Id,
            FullName = s.FullName,
            Email = s.Email,
            StudentCode = s.StudentCode,
            DateOfBirth = s.DateOfBirth,
            Gender = s.Gender,
            Program = s.Program,
            Department = s.Department,
            EnrollmentYear = s.EnrollmentYear,
            PhoneNumber = s.PhoneNumber,
            Address = s.Address,
            AcademicStatus = s.AcademicStatus,
            GPA = s.GPA
        };
    }
}
