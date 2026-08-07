using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly IUnitOfWork _unitOfWork;

        public ExamController(IExamService examService, IUnitOfWork unitOfWork)
        {
            _examService = examService;
            _unitOfWork = unitOfWork;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetString("UserId") != null;
        private string GetRole() => HttpContext.Session.GetString("UserRole") ?? "";

        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            var exams = _examService.GetAllExams();
            return View(exams);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Courses = _unitOfWork.Courses.GetAll();
            ViewBag.Faculties = _unitOfWork.Users.GetAll().Where(u => u.Role == UserRole.Faculty);

            return View(new CreateExamViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateExamViewModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = _unitOfWork.Courses.GetAll();
                ViewBag.Faculties = _unitOfWork.Users.GetAll().Where(u => u.Role == UserRole.Faculty);
                return View(model);
            }

            var (success, message) = _examService.CreateExam(model);
            if (!success)
            {
                ModelState.AddModelError("", message);
                ViewBag.Courses = _unitOfWork.Courses.GetAll();
                ViewBag.Faculties = _unitOfWork.Users.GetAll().Where(u => u.Role == UserRole.Faculty);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction("Index");
        }

        public IActionResult Details(string id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var exam = _examService.GetExamById(id);
            if (exam == null) return NotFound();

            var enrollments = _unitOfWork.Enrollments.GetByCourse(exam.CourseId);
            var eligibleStudents = enrollments
                .Where(e => e.Absences <= 6)
                .Select(e => _unitOfWork.Students.GetById(e.StudentId))
                .Where(s => s != null)
                .Select(s => s!)
                .ToList();

            var vm = new ExamDetailsViewModel
            {
                Exam = exam,
                EligibleStudents = eligibleStudents
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(string id, ExamStatus status)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin" && GetRole() != "Faculty") return RedirectToAction("AccessDenied", "Auth");

            var (success, message) = _examService.UpdateExamStatus(id, status);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Index");
        }
    }
}
