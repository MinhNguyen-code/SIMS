using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetString("UserId") != null;
        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var userId = GetUserId();
            var feedbacks = _feedbackService.GetStudentFeedbacks(userId);
            return View(feedbacks);
        }

        [HttpGet]
        public IActionResult Create(string courseId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var userId = GetUserId();
            var feedbacks = _feedbackService.GetStudentFeedbacks(userId);
            var item = feedbacks.FirstOrDefault(f => f.CourseId == courseId);

            if (item == null) return NotFound();

            var vm = new CreateFeedbackViewModel
            {
                CourseId = item.CourseId,
                CourseCode = item.CourseCode,
                CourseName = item.CourseName,
                FacultyName = item.FacultyName
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateFeedbackViewModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(model);

            var (success, message) = _feedbackService.SubmitFeedback(GetUserId(), model);
            TempData[success ? "Success" : "Error"] = message;

            if (success)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}
