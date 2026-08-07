using DayNeCu3726.Common;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IStudentService
    {
        IEnumerable<Student> GetAllStudents();

        /// <summary>
        /// Returns a bounded page of students instead of the whole table.
        /// Required by the Scalability non-functional requirement.
        /// </summary>
        PagedResult<Student> GetStudentsPaged(int pageNumber, int pageSize, string? searchTerm = null);
        Student? GetStudentById(string id);
        Student? GetStudentByEmail(string email);
        Student? GetStudentByCode(string code);
        IEnumerable<Student> SearchStudents(string query);
        (bool success, string message, Student? student) CreateStudent(StudentViewModel model);
        (bool success, string message) UpdateStudent(string id, StudentViewModel model);
        (bool success, string message) UpdateStudentStatus(string id, DayNeCu3726.Models.Enums.AcademicStatus status);
        (bool success, string message) DeleteStudent(string id);
        int GetTotalStudents();
        IEnumerable<Student> GetRecentStudents(int count = 5);
    }
}
