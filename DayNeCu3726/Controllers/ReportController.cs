using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin" || role == "Faculty";
        }

        public IActionResult Index()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Auth");
            var report = _reportService.GetSystemOverviewReport();
            return View(report);
        }

        public IActionResult PassFailRate()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Auth");
            var report = _reportService.GetPassFailReport();
            return View(report);
        }

        public IActionResult Attendance()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Auth");
            var report = _reportService.GetAttendanceWarningReport();
            return View(report);
        }

        public IActionResult Finance()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Auth");
            var report = _reportService.GetFinanceOverviewReport();
            return View(report);
        }
    }
}
