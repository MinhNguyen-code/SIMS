using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IAuthService
    {
        User? Login(string email, string password);
        (bool success, string message) Register(RegisterViewModel model);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        User? VerifyRecoveryPin(string email, string pin);
        bool ResetPassword(string userId, string newPassword);
    }
}
    