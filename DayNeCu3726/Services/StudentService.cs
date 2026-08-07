using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Patterns.Factory;
using DayNeCu3726.Common;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Security;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    /// <summary>
    /// Student Service – manages all student-related business operations.
    /// Follows Single Responsibility Principle (SRP).
    /// Open/Closed: can be extended via Decorator without modification.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public StudentService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        /// <summary>
        /// Returns one page of students, optionally filtered by a search term.
        /// <para>
        /// Added so the student list no longer calls <c>GetAll()</c>. With a large dataset that call
        /// loaded every row into memory on every page view; paging keeps both memory and response
        /// time constant as the dataset grows.
        /// </para>
        /// </summary>
        public PagedResult<Student> GetStudentsPaged(int pageNumber, int pageSize, string? searchTerm = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _unitOfWork.Students.GetPaged(pageNumber, pageSize);

            var term = searchTerm.Trim().ToLower();

            // An expression tree (not a delegate) so the provider turns this into a SQL WHERE clause.
            return _unitOfWork.Students.GetPaged(pageNumber, pageSize,
                s => s.FullName.ToLower().Contains(term) ||
                     s.Email.ToLower().Contains(term) ||
                     s.StudentCode.ToLower().Contains(term) ||
                     s.Program.ToLower().Contains(term));
        }

        public IEnumerable<Student> GetAllStudents()
            => _unitOfWork.Students.GetAll().OrderBy(s => s.FullName);

        public Student? GetStudentById(string id)
            => _unitOfWork.Students.GetById(id);

        public Student? GetStudentByEmail(string email)
            => _unitOfWork.Students.GetByEmail(email);

        public Student? GetStudentByCode(string code)
            => _unitOfWork.Students.GetByStudentCode(code);

        public IEnumerable<Student> SearchStudents(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAllStudents();

            return _unitOfWork.Students.SearchByName(query)
                .Union(_unitOfWork.Students.Find(s =>
                    s.StudentCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    s.Program.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Distinct();
        }

        public (bool success, string message, Student? student) CreateStudent(StudentViewModel model)
        {
            if (_unitOfWork.Students.GetByEmail(model.Email) != null)
                return (false, "Email already registered.", null);

            var passwordHash = _passwordHasher.Hash("Student@123");
            var student = UserFactory.CreateStudent(
                model.FullName, model.Email, passwordHash,
                model.Program, model.Department, model.DateOfBirth, model.Gender);

            student.PhoneNumber = model.PhoneNumber ?? string.Empty;
            student.Address = model.Address ?? string.Empty;
            student.EnrollmentYear = model.EnrollmentYear;
            student.AcademicStatus = model.AcademicStatus;
            student.StudentCode = _unitOfWork.Students.GenerateStudentCode(model.EnrollmentYear);

            _unitOfWork.Students.Add(student);
            _unitOfWork.Users.Add(student);
            _unitOfWork.SaveChanges();

            return (true, $"Student '{student.FullName}' registered successfully with code {student.StudentCode}.", student);
        }

        public (bool success, string message) UpdateStudent(string id, StudentViewModel model)
        {
            var student = _unitOfWork.Students.GetById(id);
            if (student == null)
                return (false, "Student not found.");

            student.FullName = model.FullName;
            student.Email = model.Email;
            student.DateOfBirth = model.DateOfBirth;
            student.Gender = model.Gender;
            student.Program = model.Program;
            student.Department = model.Department;
            student.PhoneNumber = model.PhoneNumber ?? string.Empty;
            student.Address = model.Address ?? string.Empty;
            student.AcademicStatus = model.AcademicStatus;
            student.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Students.Update(student);
            _unitOfWork.Users.Update(student);
            _unitOfWork.SaveChanges();

            return (true, "Student updated successfully.");
        }

        public (bool success, string message) UpdateStudentStatus(string id, DayNeCu3726.Models.Enums.AcademicStatus status)
        {
            var student = _unitOfWork.Students.GetById(id);
            if (student == null)
                return (false, "Student not found.");

            student.AcademicStatus = status;
            student.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Students.Update(student);
            _unitOfWork.Users.Update(student);
            _unitOfWork.SaveChanges();

            return (true, "Student status updated successfully.");
        }

        public (bool success, string message) DeleteStudent(string id)
        {
            var student = _unitOfWork.Students.GetById(id);
            if (student == null)
                return (false, "Student not found.");

            _unitOfWork.Students.Delete(id);
            _unitOfWork.Users.Delete(id);
            _unitOfWork.SaveChanges();

            return (true, "Student deleted successfully.");
        }

        public int GetTotalStudents() => _unitOfWork.Students.Count();

        public IEnumerable<Student> GetRecentStudents(int count = 5)
            => _unitOfWork.Students.GetAll()
                .OrderByDescending(s => s.CreatedAt)
                .Take(count);
    }
}
