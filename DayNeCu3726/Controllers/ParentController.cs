using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    public class ParentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IFinanceService _financeService;

        public ParentController(
            IUnitOfWork unitOfWork,
            IEnrollmentService enrollmentService,
            IFinanceService financeService)
        {
            _unitOfWork = unitOfWork;
            _enrollmentService = enrollmentService;
            _financeService = financeService;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetString("UserId") != null;
        private string GetRole() => HttpContext.Session.GetString("UserRole") ?? "";
        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        private Parent? GetCurrentParent()
        {
            var userId = GetUserId();
            return _unitOfWork.Parents.GetById(userId);
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Parent" && GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            var parent = GetCurrentParent();
            var studentId = parent?.StudentId ?? "stu-001"; // Fallback to first student if not linked
            var student = _unitOfWork.Students.GetById(studentId);

            var enrollments = _enrollmentService.GetEnrollmentsByStudent(studentId).ToList();
            var tuitions = _financeService.GetTuitionByStudent(studentId).ToList();

            var vm = new ParentDashboardViewModel
            {
                ParentName = parent?.FullName ?? "Parent",
                ParentCode = parent?.ParentCode ?? "PH00001",
                StudentInfo = new StudentViewModel
                {
                    Id = student?.Id ?? "",
                    StudentCode = student?.StudentCode ?? "",
                    FullName = student?.FullName ?? "",
                    Program = student?.Program ?? "",
                    Department = student?.Department ?? "",
                    AcademicStatus = student?.AcademicStatus ?? Models.Enums.AcademicStatus.Active
                },
                EnrolledCoursesCount = enrollments.Count,
                TotalAbsences = enrollments.Sum(e => e.Absences),
                PendingTuition = tuitions.Sum(t => t.RemainingAmount),
                AverageGrade = enrollments.Where(e => e.Grade.HasValue).Select(e => e.Grade!.Value).DefaultIfEmpty(0).Average(),
                RecentEnrollments = enrollments.Select(e => new EnrollmentViewModel
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseName = e.Course?.Name ?? "",
                    CourseCode = e.Course?.CourseCode ?? "",
                    Grade = e.Grade,
                    LetterGrade = e.LetterGrade,
                    Absences = e.Absences,
                    TotalSessions = e.TotalSessions,
                    FacultyName = e.Course?.FacultyName ?? "",
                    Status = e.Status
                }).ToList(),
                Tuitions = tuitions
            };

            return View(vm);
        }

        public IActionResult Attendance()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var parent = GetCurrentParent();
            var studentId = parent?.StudentId ?? "stu-001";
            var enrollments = _enrollmentService.GetEnrollmentsByStudent(studentId);

            var vm = enrollments.Select(e => new EnrollmentViewModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseName = e.Course?.Name ?? "",
                CourseCode = e.Course?.CourseCode ?? "",
                Absences = e.Absences,
                TotalSessions = e.TotalSessions,
                AttendancePattern = e.AttendancePattern,
                FacultyName = e.Course?.FacultyName ?? "",
                Status = e.Status
            });

            return View(vm);
        }

        public IActionResult Grades()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var parent = GetCurrentParent();
            var studentId = parent?.StudentId ?? "stu-001";
            var enrollments = _enrollmentService.GetEnrollmentsByStudent(studentId);

            var vm = enrollments.Select(e => new EnrollmentViewModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseName = e.Course?.Name ?? "",
                CourseCode = e.Course?.CourseCode ?? "",
                Credits = e.Course?.Credits ?? 0,
                Grade = e.Grade,
                LetterGrade = e.LetterGrade,
                Status = e.Status,
                Schedule = e.Course?.Semester ?? "2025-1"
            });

            return View(vm);
        }

        public IActionResult Finance()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var parent = GetCurrentParent();
            var studentId = parent?.StudentId ?? "stu-001";
            var tuitions = _financeService.GetTuitionByStudent(studentId);

            return View(tuitions);
        }
    }
}
