namespace DayNeCu3726.Models.ViewModels
{
    public class ParentDashboardViewModel
    {
        public string ParentName { get; set; } = string.Empty;
        public string ParentCode { get; set; } = string.Empty;
        public StudentViewModel StudentInfo { get; set; } = new();
        public int EnrolledCoursesCount { get; set; }
        public int TotalAbsences { get; set; }
        public decimal PendingTuition { get; set; }
        public double AverageGrade { get; set; }
        public List<EnrollmentViewModel> RecentEnrollments { get; set; } = new();
        public List<TuitionViewModel> Tuitions { get; set; } = new();
    }
}
