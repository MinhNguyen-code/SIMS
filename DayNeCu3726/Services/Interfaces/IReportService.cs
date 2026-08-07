using System.Collections.Generic;
using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IReportService
    {
        SystemReportViewModel GetSystemOverviewReport();
        IEnumerable<CoursePassFailReportViewModel> GetPassFailReport();
        IEnumerable<AttendanceWarningReportViewModel> GetAttendanceWarningReport();
        FinanceReportViewModel GetFinanceOverviewReport();
    }
}
