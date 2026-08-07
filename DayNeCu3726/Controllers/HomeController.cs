using DayNeCu3726.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DayNeCu3726.Controllers
{
    public class HomeController : Controller
    {
        // Entry point: redirect to Dashboard if logged in, otherwise to Login
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Dashboard");
            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
