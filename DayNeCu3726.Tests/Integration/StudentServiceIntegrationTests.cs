using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Patterns.Decorator;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services;
using DayNeCu3726.Services.Interfaces;
using DayNeCu3726.Tests.TestDoubles;

namespace DayNeCu3726.Tests.Integration
{
    /// <summary>
    /// Integration tests for <see cref="StudentService"/> against a real database context,
    /// including the audit <see cref="AuditStudentServiceDecorator"/> wrapper used in production.
    /// </summary>
    public class StudentServiceIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FakePasswordHasher _hasher = new();
        private readonly IStudentService _service;

        public StudentServiceIntegrationTests()
        {
            _unitOfWork = TestData.CreateUnitOfWork(out _context);
            _service = new StudentService(_unitOfWork, _hasher);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            GC.SuppressFinalize(this);
        }

        private void SeedStudents(int count, string program = "Computer Science")
        {
            for (var i = 1; i <= count; i++)
            {
                _unitOfWork.Students.Add(TestData.CreateStudent(
                    fullName: $"Student {i:D3}",
                    email: $"student{i}@sims.edu",
                    program: program,
                    studentCode: $"BH{i:D5}"));
            }

            _unitOfWork.SaveChanges();
        }

        [Fact]
        public void CreateStudent_StoresAHashedPassword()
        {
            var (success, _, student) = _service.CreateStudent(new StudentViewModel
            {
                FullName = "Nguyen Van A",
                Email = "create@sims.edu",
                Program = "Computer Science",
                Department = "Computing",
                Password = "MySecret@1",
                EnrollmentYear = 2025,
                DateOfBirth = new DateTime(2004, 1, 1),
                Gender = "Male"
            });

            Assert.True(success);
            Assert.NotNull(student);
            Assert.NotEqual("MySecret@1", student!.PasswordHash);
            Assert.True(_hasher.Verify("MySecret@1", student.PasswordHash));
        }

        [Fact]
        public void CreateStudent_DuplicateEmail_IsRejected()
        {
            SeedStudents(1);

            var (success, message, _) = _service.CreateStudent(new StudentViewModel
            {
                FullName = "Duplicate",
                Email = "student1@sims.edu",
                Program = "Computer Science",
                EnrollmentYear = 2025
            });

            Assert.False(success);
            Assert.Contains("already registered", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateStudent_AssignsAStudentCodeAutomatically()
        {
            var (_, _, student) = _service.CreateStudent(new StudentViewModel
            {
                FullName = "Auto Code",
                Email = "autocode@sims.edu",
                Program = "Computer Science",
                EnrollmentYear = 2025
            });

            Assert.NotNull(student);
            Assert.StartsWith("BH", student!.StudentCode);
        }

        /// <summary>
        /// The paging behaviour that replaced the unbounded <c>GetAllStudents()</c> call in the
        /// student list screen.
        /// </summary>
        [Fact]
        public void GetStudentsPaged_ReturnsOnlyTheRequestedPage()
        {
            SeedStudents(45);

            var page = _service.GetStudentsPaged(pageNumber: 2, pageSize: 20);

            Assert.Equal(20, page.Items.Count);
            Assert.Equal(45, page.TotalCount);
            Assert.Equal(3, page.TotalPages);
        }

        [Fact]
        public void GetStudentsPaged_WithSearchTerm_FiltersResults()
        {
            SeedStudents(20, program: "Computer Science");
            _unitOfWork.Students.Add(TestData.CreateStudent(
                fullName: "Unique Person", email: "unique@sims.edu",
                program: "Cyber Security", studentCode: "BH99999"));
            _unitOfWork.SaveChanges();

            var page = _service.GetStudentsPaged(1, 20, "Cyber");

            Assert.Equal(1, page.TotalCount);
            Assert.Equal("Unique Person", page.Items[0].FullName);
        }

        [Theory]
        [InlineData("unique@sims.edu")]
        [InlineData("BH99999")]
        [InlineData("Unique")]
        public void GetStudentsPaged_SearchesNameEmailAndCode(string term)
        {
            _unitOfWork.Students.Add(TestData.CreateStudent(
                fullName: "Unique Person", email: "unique@sims.edu",
                program: "Cyber Security", studentCode: "BH99999"));
            _unitOfWork.SaveChanges();

            Assert.Equal(1, _service.GetStudentsPaged(1, 20, term).TotalCount);
        }

        [Fact]
        public void GetStudentsPaged_BlankSearchTerm_ReturnsEveryStudent()
        {
            SeedStudents(12);

            Assert.Equal(12, _service.GetStudentsPaged(1, 50, "   ").TotalCount);
        }

        [Fact]
        public void UpdateStudentStatus_PersistsTheNewStatus()
        {
            SeedStudents(1);
            var student = _unitOfWork.Students.GetByEmail("student1@sims.edu")!;

            var (success, _) = _service.UpdateStudentStatus(student.Id, AcademicStatus.Suspended);

            Assert.True(success);
            Assert.Equal(AcademicStatus.Suspended, _service.GetStudentById(student.Id)!.AcademicStatus);
        }

        [Fact]
        public void UpdateStudentStatus_UnknownId_ReturnsFailureInsteadOfThrowing()
        {
            var (success, message) = _service.UpdateStudentStatus("does-not-exist", AcademicStatus.Active);

            Assert.False(success);
            Assert.Contains("not found", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void DeleteStudent_RemovesTheRecord()
        {
            SeedStudents(1);
            var student = _unitOfWork.Students.GetByEmail("student1@sims.edu")!;

            var (success, _) = _service.DeleteStudent(student.Id);

            Assert.True(success);
            Assert.Null(_service.GetStudentById(student.Id));
        }

        /// <summary>
        /// Confirms the Decorator adds auditing while leaving the wrapped behaviour unchanged —
        /// the Open/Closed Principle demonstrated at runtime rather than only in a diagram.
        /// </summary>
        [Fact]
        public void AuditDecorator_LogsTheOperationAndReturnsTheSameResult()
        {
            SeedStudents(3);
            var decorated = new AuditStudentServiceDecorator(_service);

            var page = decorated.GetStudentsPaged(1, 10);

            Assert.Equal(3, page.TotalCount);
            Assert.Contains(AuditStudentServiceDecorator.AuditLog,
                entry => entry.Contains("GetStudentsPaged", StringComparison.Ordinal));
        }

        [Fact]
        public void AuditDecorator_DoesNotAlterCreateBehaviour()
        {
            var decorated = new AuditStudentServiceDecorator(_service);

            var (success, _, student) = decorated.CreateStudent(new StudentViewModel
            {
                FullName = "Decorated Create",
                Email = "decorated@sims.edu",
                Program = "Computer Science",
                EnrollmentYear = 2025
            });

            Assert.True(success);
            Assert.NotNull(student);
            Assert.NotNull(_service.GetStudentByEmail("decorated@sims.edu"));
        }

        [Fact]
        public void Constructor_NullDependencies_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new StudentService(null!, _hasher));
            Assert.Throws<ArgumentNullException>(() => new StudentService(_unitOfWork, null!));
        }
    }
}
