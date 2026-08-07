using DayNeCu3726.Common;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Patterns.Decorator
{
    /// <summary>
    /// Decorator Pattern – wraps IStudentService to add cross-cutting audit logging
    /// without modifying the original StudentService class.
    /// Demonstrates Open/Closed Principle: extend behavior without modifying existing code.
    /// </summary>
    public class AuditStudentServiceDecorator : IStudentService
    {
        private readonly IStudentService _inner;
        private const int MaxRetainedEntries = 500;
        private static readonly List<string> _auditLog = new();

        public AuditStudentServiceDecorator(IStudentService inner)
        {
            _inner = inner;
        }

        public static IReadOnlyList<string> AuditLog
        {
            get { lock (_auditLog) { return _auditLog.ToList(); } }
        }

        private void Log(string action, string detail)
        {
            var entry = $"[AUDIT] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {action} | {detail}";

            lock (_auditLog)
            {
                _auditLog.Add(entry);

                // The log is static and shared by every request, so it must be bounded;
                // otherwise a long-running server leaks memory one audit line at a time.
                if (_auditLog.Count > MaxRetainedEntries)
                    _auditLog.RemoveRange(0, _auditLog.Count - MaxRetainedEntries);
            }
        }

        public IEnumerable<Student> GetAllStudents()
        {
            Log("READ", "GetAllStudents called");
            return _inner.GetAllStudents();
        }

        public Student? GetStudentById(string id)
        {
            Log("READ", $"GetStudentById: {id}");
            return _inner.GetStudentById(id);
        }

        public Student? GetStudentByEmail(string email)
        {
            Log("READ", $"GetStudentByEmail: {email}");
            return _inner.GetStudentByEmail(email);
        }

        public Student? GetStudentByCode(string code)
        {
            Log("READ", $"GetStudentByCode: {code}");
            return _inner.GetStudentByCode(code);
        }

        public IEnumerable<Student> SearchStudents(string query)
        {
            Log("READ", $"SearchStudents: '{query}'");
            return _inner.SearchStudents(query);
        }

        public (bool success, string message, Student? student) CreateStudent(StudentViewModel model)
        {
            Log("CREATE", $"CreateStudent: {model.FullName} | {model.Email}");
            var result = _inner.CreateStudent(model);
            Log("CREATE_RESULT", $"Success={result.success} | {result.message}");
            return result;
        }

        public (bool success, string message) UpdateStudent(string id, StudentViewModel model)
        {
            Log("UPDATE", $"UpdateStudent: {id} | {model.FullName}");
            var result = _inner.UpdateStudent(id, model);
            Log("UPDATE_RESULT", $"Success={result.success} | {result.message}");
            return result;
        }

        public (bool success, string message) UpdateStudentStatus(string id, DayNeCu3726.Models.Enums.AcademicStatus status)
        {
            Log("UPDATE_STATUS", $"UpdateStudentStatus: {id} | {status}");
            var result = _inner.UpdateStudentStatus(id, status);
            Log("UPDATE_STATUS_RESULT", $"Success={result.success} | {result.message}");
            return result;
        }


        public (bool success, string message) DeleteStudent(string id)
        {
            Log("DELETE", $"DeleteStudent: {id}");
            var result = _inner.DeleteStudent(id);
            Log("DELETE_RESULT", $"Success={result.success} | {result.message}");
            return result;
        }

        public PagedResult<Student> GetStudentsPaged(int pageNumber, int pageSize, string? searchTerm = null)
        {
            Log("READ", $"GetStudentsPaged: page={pageNumber}, size={pageSize}, search='{searchTerm}'");
            return _inner.GetStudentsPaged(pageNumber, pageSize, searchTerm);
        }

        public int GetTotalStudents() => _inner.GetTotalStudents();

        public IEnumerable<Student> GetRecentStudents(int count = 5)
            => _inner.GetRecentStudents(count);
    }
}
