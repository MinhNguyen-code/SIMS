using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Patterns.Observer
{
    /// <summary>
    /// Observer Pattern – Subject/Publisher class.
    /// Manages a list of observers and notifies them when enrollment events occur.
    /// </summary>
    public class EnrollmentEventPublisher
    {
        private readonly List<IEnrollmentObserver> _observers = new();

        public void Subscribe(IEnrollmentObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unsubscribe(IEnrollmentObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyEnrolled(Student student, Course course)
        {
            foreach (var observer in _observers)
                observer.OnStudentEnrolled(student, course);
        }

        public void NotifyDropped(Student student, Course course)
        {
            foreach (var observer in _observers)
                observer.OnStudentDropped(student, course);
        }

        public void NotifyGradeUpdated(Student student, Course course, double grade)
        {
            foreach (var observer in _observers)
                observer.OnGradeUpdated(student, course, grade);
        }
    }
}
