using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Patterns.Observer
{
    /// <summary>
    /// Observer Pattern – Observer interface.
    /// All enrollment event subscribers must implement this contract.
    /// </summary>
    public interface IEnrollmentObserver
    {
        void OnStudentEnrolled(Student student, Course course);
        void OnStudentDropped(Student student, Course course);
        void OnGradeUpdated(Student student, Course course, double grade);
    }
}
