using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    public class EFCourseRepository : EFRepository<Course>, ICourseRepository
    {
        public EFCourseRepository(AppDbContext context) : base(context)
        {
        }

        public Course? GetByCourseCode(string courseCode)
        {
            return _dbSet.FirstOrDefault(c => c.CourseCode.ToLower() == courseCode.ToLower());
        }

        public IEnumerable<Course> GetByFaculty(string facultyId)
        {
            return _dbSet.Where(c => c.FacultyId == facultyId).ToList();
        }

        public IEnumerable<Course> GetByStatus(CourseStatus status)
        {
            return _dbSet.Where(c => c.Status == status).ToList();
        }

        public IEnumerable<Course> GetBySemester(string semester)
        {
            return _dbSet.Where(c => c.Semester.ToLower() == semester.ToLower()).ToList();
        }

        public IEnumerable<Course> SearchByName(string name)
        {
            return _dbSet.Where(c => Microsoft.EntityFrameworkCore.EF.Functions.Like(c.Name, $"%{name}%") || 
                                     Microsoft.EntityFrameworkCore.EF.Functions.Like(c.CourseCode, $"%{name}%")).ToList();
        }
    }
}
