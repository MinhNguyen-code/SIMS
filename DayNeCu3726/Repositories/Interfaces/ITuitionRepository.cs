using System.Collections.Generic;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Tuition entity
    /// </summary>
    public interface ITuitionRepository : IRepository<Tuition>
    {
        IEnumerable<Tuition> GetByStudent(string studentId);
        Tuition? GetByStudentAndSemester(string studentId, string semester);
        IEnumerable<Tuition> GetByStatus(TuitionStatus status);
        IEnumerable<Tuition> GetBySemester(string semester);
        IEnumerable<Tuition> GetOverdueTuitions();
    }
}
