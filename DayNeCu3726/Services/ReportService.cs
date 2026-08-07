using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public SystemReportViewModel GetSystemOverviewReport()
        {
            var students = _unitOfWork.Students.GetAll();
            var courses = _unitOfWork.Courses.GetAll();
            var enrollments = _unitOfWork.Enrollments.GetAll();
            var tuitions = _unitOfWork.Tuitions.GetAll();

            var passFailReports = GetPassFailReport().ToList();
            var totalTuitionBilled = tuitions.Sum(t => t.TotalAmount);
            var totalTuitionCollected = tuitions.Sum(t => t.PaidAmount);
            var totalTuitionDebt = totalTuitionBilled - totalTuitionCollected;

            int totalEnrolled = enrollments.Count();
            int passedStudents = enrollments.Count(e => e.Status == EnrollmentStatus.Completed || e.LetterGrade == "P" || e.LetterGrade == "M" || e.LetterGrade == "D");
            double overallPassRate = totalEnrolled > 0 ? (double)passedStudents / totalEnrolled * 100 : 0;

            return new SystemReportViewModel
            {
                TotalStudents = students.Count(),
                ActiveStudents = students.Count(s => s.AcademicStatus == AcademicStatus.Active),
                GraduatedStudents = students.Count(s => s.AcademicStatus == AcademicStatus.Graduated),
                TotalCourses = courses.Count(),
                TotalEnrollments = totalEnrolled,
                OverallPassRate = overallPassRate,
                TotalTuitionBilled = totalTuitionBilled,
                TotalTuitionCollected = totalTuitionCollected,
                TotalTuitionDebt = totalTuitionDebt,
                CourseReports = passFailReports.Take(5).ToList(),
                AttendanceWarnings = GetAttendanceWarningReport().Take(5).ToList()
            };
        }

        public IEnumerable<CoursePassFailReportViewModel> GetPassFailReport()
        {
            var courses = _unitOfWork.Courses.GetAll();
            var enrollments = _unitOfWork.Enrollments.GetAll().ToList();

            var report = courses.Select(c => {
                var courseEnrollments = enrollments.Where(e => e.CourseId == c.CourseId).ToList();
                var distinction = courseEnrollments.Count(e => e.LetterGrade == "D");
                var merit = courseEnrollments.Count(e => e.LetterGrade == "M");
                var pass = courseEnrollments.Count(e => e.LetterGrade == "P");
                var passed = distinction + merit + pass;
                var failed = courseEnrollments.Count - passed;

                return new CoursePassFailReportViewModel
                {
                    CourseId = c.CourseId,
                    CourseCode = c.CourseCode,
                    CourseName = c.Name,
                    FacultyName = c.FacultyName ?? "N/A",
                    TotalEnrolled = courseEnrollments.Count,
                    PassedCount = passed,
                    FailedCount = failed,
                    DistinctionCount = distinction,
                    MeritCount = merit,
                    PassCount = pass
                };
            });

            return report.OrderByDescending(r => r.TotalEnrolled).ToList();
        }

        public IEnumerable<AttendanceWarningReportViewModel> GetAttendanceWarningReport()
        {
            var enrollments = _unitOfWork.Enrollments.GetAll();
            var courses = _unitOfWork.Courses.GetAll().ToDictionary(c => c.CourseId, c => c);
            var students = _unitOfWork.Students.GetAll().ToDictionary(s => s.Id, s => s);

            var report = enrollments.Where(e => e.Absences > 6)
                .Select(e => new AttendanceWarningReportViewModel
                {
                    StudentId = e.StudentId,
                    StudentCode = students.ContainsKey(e.StudentId) ? students[e.StudentId].StudentCode : "",
                    StudentName = students.ContainsKey(e.StudentId) ? students[e.StudentId].FullName : "",
                    CourseCode = courses.ContainsKey(e.CourseId) ? courses[e.CourseId].CourseCode : "",
                    CourseName = courses.ContainsKey(e.CourseId) ? courses[e.CourseId].Name : "",
                    Absences = e.Absences,
                    TotalSessions = e.TotalSessions > 0 ? e.TotalSessions : 30
                });

            return report.OrderByDescending(r => r.Absences).ToList();
        }

        public FinanceReportViewModel GetFinanceOverviewReport()
        {
            var tuitions = _unitOfWork.Tuitions.GetAll().ToList();
            var students = _unitOfWork.Students.GetAll().ToDictionary(s => s.Id, s => s);

            var tuitionViewModels = tuitions.Select(t => new TuitionViewModel
            {
                TuitionId = t.TuitionId,
                StudentId = t.StudentId,
                StudentName = students.ContainsKey(t.StudentId) ? students[t.StudentId].FullName : "",
                StudentCode = students.ContainsKey(t.StudentId) ? students[t.StudentId].StudentCode : "",
                Semester = t.Semester,
                TotalAmount = t.TotalAmount,
                PaidAmount = t.PaidAmount,
                Status = t.Status,
                DueDate = t.DueDate
            }).ToList();

            return new FinanceReportViewModel
            {
                TotalBilled = tuitions.Sum(t => t.TotalAmount),
                TotalCollected = tuitions.Sum(t => t.PaidAmount),
                TotalDebt = tuitions.Sum(t => t.TotalAmount) - tuitions.Sum(t => t.PaidAmount),
                Tuitions = tuitionViewModels.OrderByDescending(t => t.RemainingAmount).ToList()
            };
        }
    }
}
