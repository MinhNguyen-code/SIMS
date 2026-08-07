using Microsoft.AspNetCore.Mvc;
using DayNeCu3726.Services.Interfaces;
using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var announcements = _announcementService.GetAllAnnouncements()
                                .OrderByDescending(a => a.IsPinned)
                                .ThenByDescending(a => a.CreatedAt);
            return View(announcements);
        }

        public IActionResult Details(string id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var announcement = _announcementService.GetAnnouncementById(id);
            if (announcement == null)
            {
                TempData["Error"] = "Announcement not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(announcement);
        }

        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Faculty")
            {
                TempData["Error"] = "You do not have permission to create announcements.";
                return RedirectToAction(nameof(Index));
            }

            return View(new CreateAnnouncementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateAnnouncementViewModel model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Faculty")
            {
                TempData["Error"] = "You do not have permission to create announcements.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                var authorId = HttpContext.Session.GetString("UserId");
                var authorName = HttpContext.Session.GetString("FullName") ?? "Unknown";

                var (success, message) = _announcementService.CreateAnnouncement(authorId!, authorName, model);
                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", message);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Faculty")
            {
                TempData["Error"] = "You do not have permission to delete announcements.";
                return RedirectToAction(nameof(Index));
            }

            var (success, message) = _announcementService.DeleteAnnouncement(id);
            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
