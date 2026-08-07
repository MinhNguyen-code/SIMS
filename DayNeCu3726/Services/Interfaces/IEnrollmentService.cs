using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IEnrollmentService
    {
        IEnumerable<Enrollment> GetEnrollmentsByStudent(string studentId);
        IEnumerable<Enrollment> GetEnrollmentsByCourse(string courseId);
        (bool success, string message) EnrollStudent(string studentId, string courseId);
        (bool success, string message) DropCourse(string studentId, string courseId);
        (bool success, string message) UpdateGrade(string enrollmentId, double grade, int absences, string? remarks = null);
        (bool success, string message) UpdateAttendance(string enrollmentId, string attendancePattern);
        bool IsStudentEnrolled(string studentId, string courseId);
        int GetTotalEnrollments();
        IEnumerable<Enrollment> GetRecentEnrollments(int count = 5);
    }
}
