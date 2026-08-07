using System;
using System.Collections.Generic;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Exam entity
    /// </summary>
    public interface IExamRepository : IRepository<Exam>
    {
        IEnumerable<Exam> GetByCourse(string courseId);
        IEnumerable<Exam> GetBySemester(string semester);
        IEnumerable<Exam> GetByStatus(ExamStatus status);
        IEnumerable<Exam> GetBySupervisor(string facultyId);
        IEnumerable<Exam> GetByDate(DateTime date);
    }
}
