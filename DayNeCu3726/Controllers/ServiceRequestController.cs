using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetString("UserId") != null;
        private string GetRole() => HttpContext.Session.GetString("UserRole") ?? "";
        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var role = GetRole();
            var userId = GetUserId();

            IEnumerable<ServiceRequestViewModel> requests;
            if (role == "Admin")
            {
                requests = _serviceRequestService.GetAllRequests();
            }
            else
            {
                requests = _serviceRequestService.GetRequestsByStudent(userId);
            }

            return View(requests);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            return View(new CreateServiceRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateServiceRequestViewModel model)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(model);

            var (success, message) = _serviceRequestService.CreateRequest(GetUserId(), model);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Index");
        }

        public IActionResult Details(string id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var request = _serviceRequestService.GetRequestById(id);
            if (request == null) return NotFound();

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Process(string id, RequestStatus status, string adminResponse)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");
            if (GetRole() != "Admin") return RedirectToAction("AccessDenied", "Auth");

            var (success, message) = _serviceRequestService.UpdateStatus(id, status, adminResponse, GetUserId());
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Details", new { id });
        }
    }
}
