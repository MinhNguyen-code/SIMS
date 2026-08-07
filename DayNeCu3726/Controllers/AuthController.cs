using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    /// <summary>
    /// Handles user authentication: Login, Logout, Register.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _authService.Login(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Store session
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            TempData["Success"] = $"Welcome back, {user.FullName}!";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = _authService.Register(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = "Account created successfully. Please login.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _authService.VerifyRecoveryPin(model.Email, model.RecoveryPin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Email or Recovery PIN.");
                return View(model);
            }

            // In a real app, generate a secure token. For demo, we just pass the user ID.
            return RedirectToAction("ResetPassword", new { token = user.Id });
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = _authService.ResetPassword(model.Token, model.NewPassword);
            if (success)
            {
                TempData["Success"] = "Your password has been reset successfully. Please login.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Failed to reset password. Please try again.");
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
