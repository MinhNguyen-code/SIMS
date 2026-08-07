using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    /// <summary>
    /// Enrollment controller.
    /// Student: Enroll/Drop courses, view own transcript.
    /// Faculty: View enrolled students, enter grades.
    /// Admin: View all enrollments.
    /// </summary>
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;
        private readonly DayNeCu3726.Repositories.Interfaces.IUnitOfWork _unitOfWork;

        public EnrollmentController(IEnrollmentService enrollmentService,
            ICourseService courseService, IStudentService studentService,
            DayNeCu3726.Repositories.Interfaces.IUnitOfWork unitOfWork)
        {
            _enrollmentService = enrollmentService;
            _courseService = courseService;
            _studentService = studentService;
            _unitOfWork = unitOfWork;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetString("UserId") != null;
        private string GetRole() => HttpContext.Session.GetString("UserRole") ?? "";
        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        // Student: view my courses / transcript
        public IActionResult MyEnrollments()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var userId = GetUserId();
            var enrollments = _enrollmentService.GetEnrollmentsByStudent(userId);

            var vm = enrollments.Select(e => new EnrollmentViewModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = e.Student?.FullName ?? "",
                CourseId = e.CourseId,
                CourseName = e.Course?.Name ?? "",
                CourseCode = e.Course?.CourseCode ?? "",
                Credits = e.Course?.Credits ?? 0,
                EnrollDate = e.EnrollDate,
                Grade = e.Grade,
                LetterGrade = e.LetterGrade,
                Absences = e.Absences,
                TotalSessions = e.TotalSessions,
                AttendancePattern = e.AttendancePattern,
                DayPattern = e.Course?.DayPattern ?? "",
                SlotGroup = e.Course?.SlotGroup ?? 1,
                Status = e.Status,
                FacultyName = e.Course?.FacultyName ?? "",
                Schedule = e.Course?.Schedule ?? ""
            });

            return View(vm);
        }

        [HttpGet]
        [Route("Enrollment/Manage/{courseId?}")]
        public IActionResult Manage(string courseId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            var course = _courseService.GetCourseById(courseId);
            if (course == null) return NotFound();

            var enrollments = _enrollmentService.GetEnrollmentsByCourse(courseId).ToList();
            var allStudents = _studentService.GetAllStudents();
            var enrolledStudentIds = enrollments.Select(e => e.StudentId).ToHashSet();
            
            var availableStudents = allStudents.Where(s => !enrolledStudentIds.Contains(s.Id)).ToList();

            ViewBag.Course = course;
            ViewBag.AvailableStudents = availableStudents;
            ViewBag.Enrollments = enrollments;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Manage(string courseId, string studentId, string actionType)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            if (actionType == "Enroll")
            {
                var (success, message) = _enrollmentService.EnrollStudent(studentId, courseId);
                TempData[success ? "Success" : "Error"] = message;
            }
            else if (actionType == "Drop")
            {
                var (success, message) = _enrollmentService.DropCourse(studentId, courseId);
                TempData[success ? "Success" : "Error"] = message;
            }

            return RedirectToAction("Manage", new { courseId });
        }

        [HttpGet]
        [Route("Enrollment/ManageStudent/{studentId?}")]
        public IActionResult ManageStudent(string studentId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            var student = _studentService.GetStudentById(studentId);
            if (student == null) return NotFound();

            var enrollments = _enrollmentService.GetEnrollmentsByStudent(studentId).ToList();
            var allCourses = _courseService.GetAllCourses();
            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToHashSet();
            
            var availableCourses = allCourses.Where(c => !enrolledCourseIds.Contains(c.CourseId)).ToList();

            ViewBag.Student = student;
            ViewBag.AvailableCourses = availableCourses;
            ViewBag.Enrollments = enrollments;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ManageStudent(string studentId, string courseId, string actionType)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            if (actionType == "Enroll")
            {
                var (success, message) = _enrollmentService.EnrollStudent(studentId, courseId);
                TempData[success ? "Success" : "Error"] = message;
            }
            else if (actionType == "Drop")
            {
                var (success, message) = _enrollmentService.DropCourse(studentId, courseId);
                TempData[success ? "Success" : "Error"] = message;
            }

            return RedirectToAction("ManageStudent", new { studentId });
        }

        // Faculty: view students in a course and enter grades
        [Route("Enrollment/CourseRoster/{courseId?}")]
        public IActionResult CourseRoster(string courseId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            var course = _courseService.GetCourseById(courseId);
            if (course == null) return NotFound();

            var enrollments = _enrollmentService.GetEnrollmentsByCourse(courseId);

            var vm = new CourseEnrollmentViewModel
            {
                Course = new CourseViewModel
                {
                    CourseId = course.CourseId, CourseCode = course.CourseCode,
                    Name = course.Name, FacultyName = course.FacultyName,
                    MaxEnrollment = course.MaxEnrollment, CurrentEnrollment = course.CurrentEnrollment,
                    Schedule = course.Schedule, Semester = course.Semester
                },
                Enrollments = enrollments.Select(e => new EnrollmentViewModel
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentId = e.StudentId,
                    StudentName = e.Student?.FullName ?? "",
                    StudentCode = e.Student is Models.Entities.Student s ? s.StudentCode : "",
                    CourseId = e.CourseId,
                    CourseName = course.Name,
                    CourseCode = course.CourseCode,
                    EnrollDate = e.EnrollDate,
                    Grade = e.Grade,
                    LetterGrade = e.LetterGrade,
                    Absences = e.Absences,
                    TotalSessions = e.TotalSessions,
                    AttendancePattern = e.AttendancePattern,
                    DayPattern = course.DayPattern,
                    SlotGroup = course.SlotGroup,
                    Status = e.Status
                })
            };

            return View(vm);
        }

        // Faculty: grade entry form
        [HttpGet]
        [Route("Enrollment/Grade/{enrollmentId?}")]
        public IActionResult Grade(string enrollmentId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            var enrollments = _enrollmentService.GetEnrollmentsByStudent("dummy");
            // We need to find enrollment by ID
            // Get all enrollments for all students – search in all courses
            Enrollment? enrollment = null;
            var allCourses = _courseService.GetAllCourses();
            foreach (var c in allCourses)
            {
                var e = _enrollmentService.GetEnrollmentsByCourse(c.CourseId)
                    .FirstOrDefault(x => x.EnrollmentId == enrollmentId);
                if (e != null) { enrollment = e; break; }
            }

            if (enrollment == null) return NotFound();

            var vm = new GradeEntryViewModel
            {
                EnrollmentId = enrollmentId,
                Grade = enrollment.Grade ?? 0,
                Absences = enrollment.Absences,
                Remarks = enrollment.Remarks,
                StudentName = enrollment.Student?.FullName ?? "",
                CourseCode = enrollment.Course?.CourseCode ?? "",
                CourseName = enrollment.Course?.Name ?? ""
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Grade(GradeEntryViewModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid) return View(model);

            var (success, message) = _enrollmentService.UpdateGrade(model.EnrollmentId, model.Grade, model.Absences, model.Remarks);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Index", "Dashboard");
        }

        // Student/Admin: View weekly timetable
        [HttpGet]
        public IActionResult Timetable()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var userId = GetUserId();
            var enrollments = _enrollmentService.GetEnrollmentsByStudent(userId);

            var vm = enrollments.Select(e => new EnrollmentViewModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = e.Student?.FullName ?? "",
                CourseId = e.CourseId,
                CourseName = e.Course?.Name ?? "",
                CourseCode = e.Course?.CourseCode ?? "",
                Credits = e.Course?.Credits ?? 0,
                DayPattern = e.Course?.DayPattern ?? "",
                SlotGroup = e.Course?.SlotGroup ?? 1,
                Schedule = e.Course?.Semester ?? "",
                Classroom = e.Course?.Classroom ?? "",
                Status = e.Status
            });

            return View(vm);
        }

        // Student/Faculty: View detailed attendance report (30 sessions)
        [HttpGet]
        [Route("Enrollment/Attendance/{enrollmentId?}")]
        public IActionResult Attendance(string enrollmentId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var enrollment = FindEnrollment(enrollmentId);
            if (enrollment == null) return NotFound();

            var vm = new EnrollmentViewModel
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                StudentName = enrollment.Student?.FullName ?? "",
                StudentCode = enrollment.Student is Models.Entities.Student s ? s.StudentCode : "",
                CourseId = enrollment.CourseId,
                CourseName = enrollment.Course?.Name ?? "",
                CourseCode = enrollment.Course?.CourseCode ?? "",
                Absences = enrollment.Absences,
                TotalSessions = enrollment.TotalSessions,
                AttendancePattern = enrollment.AttendancePattern,
                Status = enrollment.Status,
                Schedule = enrollment.Course?.Schedule ?? "",
                FacultyName = enrollment.Course?.FacultyName ?? ""
            };

            return View(vm);
        }

        // Faculty/Admin: Take attendance (update pattern)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TakeAttendance(string enrollmentId, string pattern)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            var (success, message) = _enrollmentService.UpdateAttendance(enrollmentId, pattern);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Attendance", new { enrollmentId = enrollmentId });
        }

        // Student: View BTEC Mark Report grouped by semester
        [HttpGet]
        public IActionResult MarkReport()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var userId = GetUserId();
            var enrollments = _enrollmentService.GetEnrollmentsByStudent(userId);

            var vm = enrollments.Select(e => new EnrollmentViewModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                CourseName = e.Course?.Name ?? "",
                CourseCode = e.Course?.CourseCode ?? "",
                Credits = e.Course?.Credits ?? 0,
                EnrollDate = e.EnrollDate,
                Grade = e.Grade,
                LetterGrade = e.LetterGrade,
                Absences = e.Absences,
                TotalSessions = e.TotalSessions,
                Status = e.Status,
                Schedule = e.Course?.Schedule ?? "",
                DayPattern = e.Course?.Semester ?? "" // Use DayPattern as Semester temporary mapping or Course.Semester
            }).ToList();

            // We can resolve course semester directly
            foreach(var item in vm)
            {
                var original = enrollments.First(x => x.EnrollmentId == item.EnrollmentId);
                item.Schedule = original.Course?.Semester ?? "Unknown Semester"; // Pass Semester through Schedule field to group
            }

            return View(vm);
        }

        private Enrollment? FindEnrollment(string enrollmentId)
        {
            var allCourses = _courseService.GetAllCourses();
            foreach (var c in allCourses)
            {
                var e = _enrollmentService.GetEnrollmentsByCourse(c.CourseId)
                    .FirstOrDefault(x => x.EnrollmentId == enrollmentId);
                if (e != null) return e;
            }
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportGrades(string courseId, IFormFile file)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid CSV file.";
                return RedirectToAction("CourseRoster", new { courseId });
            }

            try
            {
                using var reader = new System.IO.StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                int successCount = 0;
                var enrollments = _enrollmentService.GetEnrollmentsByCourse(courseId);

                // skip header if present (check if first line starts with 'StudentCode')
                int startIndex = 0;
                if (lines.Length > 0 && lines[0].StartsWith("StudentCode", StringComparison.OrdinalIgnoreCase))
                {
                    startIndex = 1;
                }

                for (int i = startIndex; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length >= 3)
                    {
                        var studentCode = parts[0].Trim();
                        var gradeStr = parts[1].Trim();
                        var absencesStr = parts[2].Trim();
                        var remarks = parts.Length > 3 ? parts[3].Trim() : "";

                        // Find student in course enrollments
                        var enrollment = enrollments.FirstOrDefault(e => e.Student is DayNeCu3726.Models.Entities.Student s && s.StudentCode == studentCode);
                        if (enrollment != null)
                        {
                            if (double.TryParse(gradeStr, out double grade) && int.TryParse(absencesStr, out int absences))
                            {
                                var (success, _) = _enrollmentService.UpdateGrade(enrollment.EnrollmentId, grade, absences, remarks);
                                if (success) successCount++;
                            }
                        }
                    }
                }

                TempData["Success"] = $"Successfully updated {successCount} student records.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error importing grades: {ex.Message}";
            }

            return RedirectToAction("CourseRoster", new { courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinalizeGrades(string courseId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() == "Student") return RedirectToAction("AccessDenied", "Auth");

            try
            {
                var enrollments = _enrollmentService.GetEnrollmentsByCourse(courseId);
                int count = 0;
                foreach (var e in enrollments)
                {
                    if (e.Absences > 6 || (e.Grade.HasValue && e.Grade < 5))
                    {
                        e.Status = DayNeCu3726.Models.Enums.EnrollmentStatus.Failed;
                    }
                    else
                    {
                        e.Status = DayNeCu3726.Models.Enums.EnrollmentStatus.Completed;
                    }
                    count++;
                }

                _unitOfWork.SaveChanges();
                TempData["Success"] = "Course grades finalized successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error finalizing grades: {ex.Message}";
            }

            return RedirectToAction("CourseRoster", new { courseId });
        }
    }
}
