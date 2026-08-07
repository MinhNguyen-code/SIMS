using Microsoft.AspNetCore.Mvc;
using DayNeCu3726.Services.Interfaces;
using DayNeCu3726.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace DayNeCu3726.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ICourseService _courseService;
        private readonly IWebHostEnvironment _env;

        public AssignmentController(IAssignmentService assignmentService, ICourseService courseService, IWebHostEnvironment env)
        {
            _assignmentService = assignmentService;
            _courseService = courseService;
            _env = env;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null) return RedirectToAction("Login", "Auth");

            IEnumerable<AssignmentViewModel> assignments = new List<AssignmentViewModel>();

            if (role == "Student")
                assignments = _assignmentService.GetAssignmentsForStudent(userId);
            else if (role == "Faculty")
                assignments = _assignmentService.GetAssignmentsForFaculty(userId);
            else if (role == "Admin")
                assignments = _assignmentService.GetAllAssignments();

            return View(assignments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin") return Unauthorized();

            var courses = _courseService.GetAllCourses();
            ViewBag.Courses = courses;

            return View(new CreateAssignmentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateAssignmentViewModel model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin") return Unauthorized();

            if (ModelState.IsValid)
            {
                _assignmentService.CreateAssignment(model);
                TempData["Success"] = "Assignment created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Courses = _courseService.GetAllCourses();
            return View(model);
        }

        [HttpGet]
        public IActionResult Submit(string id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Student") return Unauthorized();

            var assignment = _assignmentService.GetAssignmentById(id);
            if (assignment == null) return NotFound();

            ViewBag.Assignment = assignment;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string id, IFormFile file)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            if (role != "Student") return Unauthorized();

            var assignment = _assignmentService.GetAssignmentById(id);
            if (assignment == null) return NotFound();

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                ViewBag.Assignment = assignment;
                return View();
            }

            if (file.ContentType != "application/pdf" && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Only PDF files are allowed.";
                ViewBag.Assignment = assignment;
                return View();
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "assignments");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _assignmentService.SubmitAssignment(id, userId!, uniqueFileName, file.FileName);
            TempData["Success"] = "Assignment submitted successfully.";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Submissions(string id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Faculty" && role != "Admin") return Unauthorized();

            var assignment = _assignmentService.GetAssignmentById(id);
            if (assignment == null) return NotFound();

            var submissions = _assignmentService.GetSubmissionsForAssignment(id);
            ViewBag.Assignment = assignment;
            
            return View(submissions);
        }

        [HttpGet]
        public IActionResult Grade(string id) // id is submissionId
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Faculty") return Unauthorized();

            var submission = _assignmentService.GetSubmissionById(id);
            if (submission == null) return NotFound();

            var assignment = _assignmentService.GetAssignmentById(submission.AssignmentId);
            ViewBag.Assignment = assignment;

            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Grade(string id, double grade, string feedback)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Faculty") return Unauthorized();

            if (grade < 0 || grade > 10)
            {
                TempData["Error"] = "Grade must be between 0 and 10.";
                return RedirectToAction(nameof(Grade), new { id = id });
            }

            _assignmentService.GradeSubmission(id, grade, feedback);
            TempData["Success"] = "Grade saved successfully.";

            var submission = _assignmentService.GetSubmissionById(id);
            return RedirectToAction(nameof(Submissions), new { id = submission?.AssignmentId });
        }
    }
}
