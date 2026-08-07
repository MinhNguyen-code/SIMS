using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Security;
using DayNeCu3726.Services;
using DayNeCu3726.Tests.TestDoubles;
using Moq;

namespace DayNeCu3726.Tests.Unit
{
    /// <summary>
    /// Unit tests for <see cref="AuthService"/> using Moq, a vendor-provided mocking library.
    /// <para>
    /// Every collaborator is replaced by a mock, so these tests exercise the authentication decision
    /// logic alone — no database, no cryptography cost. This is only possible because the service
    /// depends on <see cref="IUnitOfWork"/> and <see cref="IPasswordHasher"/> abstractions rather
    /// than concrete classes; the Dependency Inversion Principle is what makes the code testable.
    /// </para>
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IUserRepository> _users = new();
        private readonly Mock<IStudentRepository> _students = new();
        private readonly FakePasswordHasher _hasher = new();

        public AuthServiceTests()
        {
            _unitOfWork.SetupGet(u => u.Users).Returns(_users.Object);
            _unitOfWork.SetupGet(u => u.Students).Returns(_students.Object);
        }

        private AuthService CreateService() => new(_unitOfWork.Object, _hasher);

        private static Student ActiveStudent(string email = "a@sims.edu", string password = "Student@123") => new()
        {
            Id = "user-1",
            FullName = "Nguyen Van A",
            Email = email,
            PasswordHash = FakePasswordHasher.Prefix + password,
            IsActive = true,
            Role = UserRole.Student
        };

        [Fact]
        public void Login_WithCorrectCredentials_ReturnsUser()
        {
            var student = ActiveStudent();
            _users.Setup(r => r.GetByEmail("a@sims.edu")).Returns(student);

            var result = CreateService().Login("a@sims.edu", "Student@123");

            Assert.NotNull(result);
            Assert.Equal("user-1", result!.Id);
        }

        [Fact]
        public void Login_WithWrongPassword_ReturnsNull()
        {
            _users.Setup(r => r.GetByEmail("a@sims.edu")).Returns(ActiveStudent());

            Assert.Null(CreateService().Login("a@sims.edu", "WrongPassword"));
        }

        [Fact]
        public void Login_UnknownEmail_ReturnsNull()
        {
            _users.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((User?)null);

            Assert.Null(CreateService().Login("nobody@sims.edu", "Student@123"));
        }

        /// <summary>A deactivated account must not be able to sign in even with the right password.</summary>
        [Fact]
        public void Login_InactiveAccount_IsRefused()
        {
            var student = ActiveStudent();
            student.IsActive = false;
            _users.Setup(r => r.GetByEmail("a@sims.edu")).Returns(student);

            Assert.Null(CreateService().Login("a@sims.edu", "Student@123"));
        }

        [Theory]
        [InlineData("", "Student@123")]
        [InlineData("a@sims.edu", "")]
        [InlineData("   ", "   ")]
        public void Login_BlankCredentials_ReturnNullWithoutQueryingTheDatabase(string email, string password)
        {
            Assert.Null(CreateService().Login(email, password));

            _users.Verify(r => r.GetByEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Login_TrimsSurroundingWhitespaceFromEmail()
        {
            _users.Setup(r => r.GetByEmail("a@sims.edu")).Returns(ActiveStudent());

            Assert.NotNull(CreateService().Login("  a@sims.edu  ", "Student@123"));
        }

        /// <summary>
        /// Verifies the transparent security upgrade: a user still holding a legacy hash is
        /// re-hashed with the stronger algorithm on their next successful sign-in.
        /// </summary>
        [Fact]
        public void Login_LegacyHash_IsUpgradedAndPersisted()
        {
            var student = ActiveStudent();
            student.PasswordHash = "legacy-sha256-hash";

            var hasher = new Mock<IPasswordHasher>();
            hasher.Setup(h => h.Verify("Student@123", "legacy-sha256-hash")).Returns(true);
            hasher.Setup(h => h.NeedsUpgrade("legacy-sha256-hash")).Returns(true);
            hasher.Setup(h => h.Hash("Student@123")).Returns("PBKDF2$new");

            _users.Setup(r => r.GetByEmail("a@sims.edu")).Returns(student);

            var result = new AuthService(_unitOfWork.Object, hasher.Object).Login("a@sims.edu", "Student@123");

            Assert.NotNull(result);
            Assert.Equal("PBKDF2$new", student.PasswordHash);
            _users.Verify(r => r.Update(student), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Login_ModernHash_IsNotRewritten()
        {
            _users.Setup(r => r.GetByEmail("a@sims.edu")).Returns(ActiveStudent());

            CreateService().Login("a@sims.edu", "Student@123");

            _users.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void Register_NewEmail_CreatesUserAndSaves()
        {
            _users.Setup(r => r.EmailExists("new@sims.edu")).Returns(false);
            _students.Setup(r => r.GenerateStudentCode(It.IsAny<int>())).Returns("BH00099");

            var model = new RegisterViewModel
            {
                FullName = "Tran Thi B",
                Email = "new@sims.edu",
                Password = "Secret@123",
                Role = UserRole.Student,
                Program = "Data Science"
            };

            var (success, message) = CreateService().Register(model);

            Assert.True(success);
            Assert.Contains("successful", message, StringComparison.OrdinalIgnoreCase);
            _users.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
            _students.Verify(r => r.Add(It.IsAny<Student>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Register_DuplicateEmail_IsRejectedAndNothingIsSaved()
        {
            _users.Setup(r => r.EmailExists("taken@sims.edu")).Returns(true);

            var model = new RegisterViewModel
            {
                FullName = "Duplicate",
                Email = "taken@sims.edu",
                Password = "Secret@123",
                Role = UserRole.Student
            };

            var (success, message) = CreateService().Register(model);

            Assert.False(success);
            Assert.Contains("already registered", message, StringComparison.OrdinalIgnoreCase);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Never);
        }

        [Fact]
        public void Register_PasswordIsHashedAndNeverStoredInPlainText()
        {
            _users.Setup(r => r.EmailExists(It.IsAny<string>())).Returns(false);
            _students.Setup(r => r.GenerateStudentCode(It.IsAny<int>())).Returns("BH00100");

            User? captured = null;
            _users.Setup(r => r.Add(It.IsAny<User>())).Callback<User>(u => captured = u);

            CreateService().Register(new RegisterViewModel
            {
                FullName = "Le Van C",
                Email = "c@sims.edu",
                Password = "PlainSecret1!",
                Role = UserRole.Student
            });

            Assert.NotNull(captured);
            Assert.NotEqual("PlainSecret1!", captured!.PasswordHash);
            Assert.StartsWith(FakePasswordHasher.Prefix, captured.PasswordHash);
        }

        [Fact]
        public void Constructor_NullDependencies_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(null!, _hasher));
            Assert.Throws<ArgumentNullException>(() => new AuthService(_unitOfWork.Object, null!));
        }
    }
}
