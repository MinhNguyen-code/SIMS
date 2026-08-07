using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Patterns.Factory;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Security;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    /// <summary>
    /// Handles authentication and registration.
    /// <para>
    /// Single Responsibility Principle: the class now deals only with the authentication workflow.
    /// The cryptography it used to embed (a private <c>HashPassword</c> using SHA-256 with a shared
    /// salt) has moved behind <see cref="IPasswordHasher"/>, so the algorithm can be strengthened
    /// without editing this class and can be substituted with a fast fake in unit tests.
    /// </para>
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public User? Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _unitOfWork.Users.GetByEmail(email.Trim());
            if (user is null || !user.IsActive)
                return null;

            if (!_passwordHasher.Verify(password, user.PasswordHash))
                return null;

            UpgradeLegacyHashIfNeeded(user, password);
            return user;
        }

        public (bool success, string message) Register(RegisterViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (_unitOfWork.Users.EmailExists(model.Email))
                return (false, "Email address is already registered.");

            var hash = _passwordHasher.Hash(model.Password);
            var user = UserFactory.Create(model.Role, model.FullName, model.Email, hash);

            if (user is Student student)
            {
                student.Program = model.Program ?? "Undeclared";
                student.Department = model.Department ?? "General";
                student.PhoneNumber = model.PhoneNumber ?? string.Empty;
                student.StudentCode = _unitOfWork.Students.GenerateStudentCode(DateTime.Now.Year);
                _unitOfWork.Students.Add(student);
            }

            _unitOfWork.Users.Add(user);
            _unitOfWork.SaveChanges();

            return (true, "Registration successful.");
        }

        public User? VerifyRecoveryPin(string email, string pin)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pin))
                return null;

            var user = _unitOfWork.Users.GetByEmail(email.Trim());
            if (user is null || !user.IsActive)
                return null;

            if (user.RecoveryPin != pin)
                return null;

            return user;
        }

        public bool ResetPassword(string userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newPassword))
                return false;

            var user = _unitOfWork.Users.GetById(userId);
            if (user is null || !user.IsActive)
                return false;

            user.PasswordHash = _passwordHasher.Hash(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            _unitOfWork.SaveChanges();

            return true;
        }

        public string HashPassword(string password) => _passwordHasher.Hash(password);

        public bool VerifyPassword(string password, string hash) => _passwordHasher.Verify(password, hash);

        /// <summary>
        /// Transparently migrates an account from the old SHA-256 hash to PBKDF2 the first time the
        /// user signs in successfully. Existing accounts are therefore secured without forcing a
        /// password reset on anyone.
        /// </summary>
        private void UpgradeLegacyHashIfNeeded(User user, string password)
        {
            if (!_passwordHasher.NeedsUpgrade(user.PasswordHash))
                return;

            user.PasswordHash = _passwordHasher.Hash(password);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            _unitOfWork.SaveChanges();
        }
    }
}
