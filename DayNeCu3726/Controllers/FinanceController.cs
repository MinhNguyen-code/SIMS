using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    public class FinanceController : Controller
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetString("UserId") != null;
        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var userId = GetUserId();
            var tuitions = _financeService.GetTuitionByStudent(userId);
            return View(tuitions);
        }

        public IActionResult History(string tuitionId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var tuition = _financeService.GetTuitionById(tuitionId);
            if (tuition == null) return NotFound();

            return View(tuition);
        }

        [HttpGet]
        public IActionResult Pay(string tuitionId)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var tuition = _financeService.GetTuitionById(tuitionId);
            if (tuition == null) return NotFound();

            var vm = new PayTuitionViewModel
            {
                TuitionId = tuition.TuitionId,
                Semester = tuition.Semester,
                RemainingAmount = tuition.RemainingAmount,
                AmountToPay = tuition.RemainingAmount,
                PaymentMethod = "BankTransfer"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pay(PayTuitionViewModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            if (model.AmountToPay <= 0)
            {
                TempData["Error"] = "Payment amount must be greater than 0.";
                return View(model);
            }

            var (success, message) = _financeService.ProcessPayment(
                model.TuitionId, model.AmountToPay, model.PaymentMethod, model.Note);

            TempData[success ? "Success" : "Error"] = message;

            if (success)
            {
                return RedirectToAction("History", new { tuitionId = model.TuitionId });
            }

            return View(model);
        }
    }
}
