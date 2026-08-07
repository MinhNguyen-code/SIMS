using System.Collections.Generic;

namespace DayNeCu3726.Models.ViewModels
{
    public class SystemReportViewModel
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public double OverallPassRate { get; set; }
        public decimal TotalTuitionBilled { get; set; }
        public decimal TotalTuitionCollected { get; set; }
        public decimal TotalTuitionDebt { get; set; }
        public List<CoursePassFailReportViewModel> CourseReports { get; set; } = new();
        public List<AttendanceWarningReportViewModel> AttendanceWarnings { get; set; } = new();
    }

    public class CoursePassFailReportViewModel
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int TotalEnrolled { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public int DistinctionCount { get; set; }
        public int MeritCount { get; set; }
        public int PassCount { get; set; }
        public double PassRate => TotalEnrolled > 0 ? (double)PassedCount / TotalEnrolled * 100 : 0;
    }

    public class AttendanceWarningReportViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Absences { get; set; }
        public int TotalSessions { get; set; }
        public double AbsenceRate => TotalSessions > 0 ? (double)Absences / TotalSessions * 100 : 0;
    }
    
    public class FinanceReportViewModel
    {
        public decimal TotalBilled { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalDebt { get; set; }
        public List<TuitionViewModel> Tuitions { get; set; } = new();
    }
}
