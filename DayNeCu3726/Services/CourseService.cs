using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    /// <summary>
    /// Course Service – manages all course-related business operations.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Course> GetAllCourses()
            => _unitOfWork.Courses.GetAll()
                .OrderBy(c => c.Semester)
                .ThenBy(c => c.CourseCode);

        public Course? GetCourseById(string id)
            => _unitOfWork.Courses.GetById(id);

        public IEnumerable<Course> SearchCourses(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAllCourses();
            return _unitOfWork.Courses.SearchByName(query);
        }

        public IEnumerable<Course> GetActiveCourses()
            => _unitOfWork.Courses.GetByStatus(Models.Enums.CourseStatus.Active);

        public IEnumerable<Course> GetCoursesByFaculty(string facultyId)
            => _unitOfWork.Courses.GetByFaculty(facultyId);

        public (bool success, string message, Course? course) CreateCourse(CourseViewModel model)
        {
            if (_unitOfWork.Courses.GetByCourseCode(model.CourseCode) != null)
                return (false, $"Course code '{model.CourseCode}' already exists.", null);

            var course = new Course
            {
                CourseCode = model.CourseCode,
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                Credits = model.Credits,
                FacultyId = model.FacultyId,
                FacultyName = model.FacultyName ?? string.Empty,
                MaxEnrollment = model.MaxEnrollment,
                Schedule = model.Schedule ?? string.Empty,
                Classroom = model.Classroom ?? string.Empty,
                Semester = model.Semester ?? string.Empty,
                Status = model.Status
            };

            _unitOfWork.Courses.Add(course);
            _unitOfWork.SaveChanges();

            return (true, $"Course '{course.Name}' created successfully.", course);
        }

        public (bool success, string message) UpdateCourse(string id, CourseViewModel model)
        {
            var course = _unitOfWork.Courses.GetById(id);
            if (course == null)
                return (false, "Course not found.");

            course.Name = model.Name;
            course.Description = model.Description ?? string.Empty;
            course.Credits = model.Credits;
            course.FacultyId = model.FacultyId;
            course.FacultyName = model.FacultyName ?? string.Empty;
            course.MaxEnrollment = model.MaxEnrollment;
            course.Schedule = model.Schedule ?? string.Empty;
            course.Classroom = model.Classroom ?? string.Empty;
            course.Semester = model.Semester ?? string.Empty;
            course.Status = model.Status;

            _unitOfWork.Courses.Update(course);
            _unitOfWork.SaveChanges();

            return (true, "Course updated successfully.");
        }

        public (bool success, string message) DeleteCourse(string id)
        {
            var course = _unitOfWork.Courses.GetById(id);
            if (course == null)
                return (false, "Course not found.");

            // Check if students are enrolled
            var enrollments = _unitOfWork.Enrollments.GetByCourse(id);
            if (enrollments.Any(e => e.Status == Models.Enums.EnrollmentStatus.Enrolled))
                return (false, "Cannot delete course with active enrollments.");

            _unitOfWork.Courses.Delete(id);
            _unitOfWork.SaveChanges();

            return (true, "Course deleted successfully.");
        }

        public int GetTotalCourses() => _unitOfWork.Courses.Count();
    }
}
