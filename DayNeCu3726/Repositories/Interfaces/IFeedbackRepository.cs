using System.Collections.Generic;
using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Feedback entity
    /// </summary>
    public interface IFeedbackRepository : IRepository<Feedback>
    {
        IEnumerable<Feedback> GetByStudent(string studentId);
        IEnumerable<Feedback> GetByCourse(string courseId);
        IEnumerable<Feedback> GetByFaculty(string facultyId);
        Feedback? GetByStudentAndCourse(string studentId, string courseId);
        IEnumerable<Feedback> GetBySemester(string semester);
    }
}
