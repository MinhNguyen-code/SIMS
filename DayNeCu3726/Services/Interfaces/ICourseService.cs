using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Services.Interfaces
{
    public interface ICourseService
    {
        IEnumerable<Course> GetAllCourses();
        Course? GetCourseById(string id);
        IEnumerable<Course> SearchCourses(string query);
        IEnumerable<Course> GetActiveCourses();
        (bool success, string message, Course? course) CreateCourse(CourseViewModel model);
        (bool success, string message) UpdateCourse(string id, CourseViewModel model);
        (bool success, string message) DeleteCourse(string id);
        int GetTotalCourses();
        IEnumerable<Course> GetCoursesByFaculty(string facultyId);
    }
}
