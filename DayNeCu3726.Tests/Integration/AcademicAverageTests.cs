using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Patterns.Singleton;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services;
using DayNeCu3726.Tests.TestDoubles;

namespace DayNeCu3726.Tests.Integration
{
    /// <summary>
    /// Regression tests for the academic average calculation.
    /// <para>
    /// Under the BTEC scheme the service used to hard-set <c>student.GPA = 0.0</c>, so every
    /// dashboard reported an average of zero even for students holding marks of 9.0. These tests
    /// pin the corrected behaviour: the average is computed from the graded enrollments regardless
    /// of which grading scheme is configured.
    /// </para>
    /// </summary>
    public class AcademicAverageTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EnrollmentService _service;

        public AcademicAverageTests()
        {
            _unitOfWork = TestData.CreateUnitOfWork(out _context);
            _service = new EnrollmentService(_unitOfWork);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            GC.SuppressFinalize(this);
        }

        private Student SeedStudentWithMarks(params double?[] marks)
        {
            var student = TestData.CreateStudent(email: $"avg{Guid.NewGuid():N}@sims.edu");
            _unitOfWork.Students.Add(student);

            var course = TestData.CreateCourse();
            _unitOfWork.Courses.Add(course);
            _unitOfWork.SaveChanges();

            foreach (var mark in marks)
            {
                _unitOfWork.Enrollments.Add(new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = course.CourseId,
                    Grade = mark,
                    Status = mark.HasValue ? EnrollmentStatus.Completed : EnrollmentStatus.Enrolled
                });
            }

            _unitOfWork.SaveChanges();
            return student;
        }

        [Fact]
        public void CalculateAverageMark_ReturnsMeanOfGradedEnrollments()
        {
            var student = SeedStudentWithMarks(5.0, 7.0, 9.0);

            Assert.Equal(7.0, _service.CalculateAverageMark(student.Id));
        }

        [Fact]
        public void CalculateAverageMark_IgnoresUngradedEnrollments()
        {
            // 8.0 and 6.0 are graded; the null-marked enrollment must not drag the mean down.
            var student = SeedStudentWithMarks(8.0, 6.0, null);

            Assert.Equal(7.0, _service.CalculateAverageMark(student.Id));
        }

        [Fact]
        public void CalculateAverageMark_NoGradesYet_ReturnsZero()
        {
            var student = SeedStudentWithMarks(null, null);

            Assert.Equal(0.0, _service.CalculateAverageMark(student.Id));
        }

        [Fact]
        public void CalculateAverageMark_RoundsToTwoDecimals()
        {
            var student = SeedStudentWithMarks(7.0, 8.0, 8.0);   // mean = 7.666...

            Assert.Equal(7.67, _service.CalculateAverageMark(student.Id));
        }

        [Fact]
        public void CalculateAverageMark_UnknownStudent_ReturnsZero()
        {
            Assert.Equal(0.0, _service.CalculateAverageMark("no-such-student"));
        }

        /// <summary>
        /// The specific defect: with BTEC configured the average must still be a real number,
        /// not the hard-coded zero the previous implementation stored.
        /// </summary>
        [Fact]
        public void UnderBtecScheme_AverageIsStillCalculated()
        {
            var originalScheme = SystemConfiguration.Instance.GradingScheme;
            try
            {
                SystemConfiguration.Instance.GradingScheme = GradingScheme.Btec;

                var student = SeedStudentWithMarks(9.0, 9.0);

                Assert.Equal(9.0, _service.CalculateAverageMark(student.Id));
                Assert.NotEqual(0.0, _service.CalculateAverageMark(student.Id));
            }
            finally
            {
                SystemConfiguration.Instance.GradingScheme = originalScheme;
            }
        }

        /// <summary>
        /// The seeder must leave demo students with a populated average, otherwise the dashboard
        /// shows 0.00 for accounts that clearly have completed, graded units.
        /// </summary>
        [Fact]
        public void DataSeeder_PopulatesAcademicAveragesForGradedStudents()
        {
            using var seedContext = TestData.CreateContext();
            var seedUnitOfWork = new DayNeCu3726.Repositories.UnitOfWork(seedContext);

            DataSeeder.Seed(seedUnitOfWork);

            var gradedStudentIds = seedUnitOfWork.Enrollments.GetAll()
                .Where(e => e.Grade.HasValue)
                .Select(e => e.StudentId)
                .Distinct()
                .ToList();

            Assert.NotEmpty(gradedStudentIds);

            foreach (var id in gradedStudentIds)
            {
                var student = seedUnitOfWork.Students.GetById(id);
                Assert.NotNull(student);
                Assert.True(student!.GPA > 0,
                    $"Student {student.StudentCode} has graded enrollments but an average of {student.GPA}.");
            }
        }
    }
}
