using DayNeCu3726.Infrastructure.Authorization;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    [AuthorizeRole] // All logged-in users can access settings
    public class SettingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        public SettingsController(IUnitOfWork unitOfWork, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        private string GetUserId() => HttpContext.Session.GetString("UserId") ?? "";

        [HttpGet]
        public IActionResult Index(string tab = "profile")
        {
            var user = _unitOfWork.Users.GetById(GetUserId());
            if (user == null) return RedirectToAction("Login", "Auth");

            var vm = new SettingsViewModel
            {
                ActiveTab = tab,
                Profile = new ProfileViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(SettingsViewModel model)
        {
            model.ActiveTab = "profile";
            
            // Remove ChangePassword validation errors since we are only updating profile
            ModelState.Clear();
            TryValidateModel(model.Profile, nameof(model.Profile));

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var user = _unitOfWork.Users.GetById(GetUserId());
            if (user == null) return RedirectToAction("Login", "Auth");

            // Check if email changed and is in use
            if (user.Email != model.Profile.Email && _unitOfWork.Users.EmailExists(model.Profile.Email))
            {
                ModelState.AddModelError("Profile.Email", "This email is already in use by another account.");
                return View("Index", model);
            }

            user.FullName = model.Profile.FullName;
            user.Email = model.Profile.Email;
            user.PhoneNumber = model.Profile.PhoneNumber;
            user.Address = model.Profile.Address;

            try
            {
                _unitOfWork.Users.Update(user);
                _unitOfWork.SaveChanges();
                
                // Update Session Name
                HttpContext.Session.SetString("UserName", user.FullName);
                
                TempData["Success"] = "Profile updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating profile: {ex.Message}";
            }

            return RedirectToAction("Index", new { tab = "profile" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(SettingsViewModel model)
        {
            model.ActiveTab = "password";
            
            // Remove Profile validation errors
            ModelState.Clear();
            TryValidateModel(model.ChangePassword, nameof(model.ChangePassword));

            if (!ModelState.IsValid)
            {
                // Must reload profile info to populate the view
                var u = _unitOfWork.Users.GetById(GetUserId());
                if(u != null) {
                    model.Profile.FullName = u.FullName;
                    model.Profile.Email = u.Email;
                    model.Profile.PhoneNumber = u.PhoneNumber;
                    model.Profile.Address = u.Address;
                }
                return View("Index", model);
            }

            var user = _unitOfWork.Users.GetById(GetUserId());
            if (user == null) return RedirectToAction("Login", "Auth");

            if (!_authService.VerifyPassword(model.ChangePassword.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("ChangePassword.CurrentPassword", "Incorrect current password.");
                
                // Reload profile
                model.Profile.FullName = user.FullName;
                model.Profile.Email = user.Email;
                model.Profile.PhoneNumber = user.PhoneNumber;
                model.Profile.Address = user.Address;
                return View("Index", model);
            }

            user.PasswordHash = _authService.HashPassword(model.ChangePassword.NewPassword);

            try
            {
                _unitOfWork.Users.Update(user);
                _unitOfWork.SaveChanges();
                TempData["Success"] = "Password changed successfully. Please use your new password next time you log in.";
                // Clear the form fields
                model.ChangePassword = new ChangePasswordViewModel();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error changing password: {ex.Message}";
            }

            return RedirectToAction("Index", new { tab = "password" });
        }

        [HttpPost]
        public IActionResult ToggleTheme(string theme, string returnUrl)
        {
            // Set a cookie for the theme (light/dark)
            CookieOptions options = new CookieOptions { Expires = DateTime.Now.AddYears(1) };
            Response.Cookies.Append("theme", theme, options);
            
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return Redirect(returnUrl);
        }

        [HttpPost]
        public IActionResult ChangeLanguage(string lang, string returnUrl)
        {
            // Set a cookie for language (en/vi)
            CookieOptions options = new CookieOptions { Expires = DateTime.Now.AddYears(1) };
            Response.Cookies.Append("language", lang, options);
            
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return Redirect(returnUrl);
        }
    }
}
