using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Student-specific repository interface.
    /// Extends IRepository with domain-specific query methods.
    /// </summary>
    public interface IStudentRepository : IRepository<Student>
    {
        Student? GetByStudentCode(string studentCode);
        Student? GetByEmail(string email);
        IEnumerable<Student> GetByProgram(string program);
        IEnumerable<Student> GetByDepartment(string department);
        IEnumerable<Student> GetByEnrollmentYear(int year);
        IEnumerable<Student> SearchByName(string name);
        string GenerateStudentCode(int year);
    }
}
