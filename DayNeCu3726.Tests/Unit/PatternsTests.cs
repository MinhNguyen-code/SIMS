using DayNeCu3726.Common;
using DayNeCu3726.DataProcessing.Abstractions;
using DayNeCu3726.DataProcessing.Mapping;
using DayNeCu3726.DataProcessing.Pipeline;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Patterns.Factory;
using DayNeCu3726.Patterns.Observer;
using DayNeCu3726.Patterns.Singleton;
using DayNeCu3726.Patterns.Strategy;
using DayNeCu3726.Tests.TestDoubles;
using Moq;

namespace DayNeCu3726.Tests.Unit
{
    /// <summary>
    /// Unit tests proving each design pattern used in the application behaves as intended.
    /// Covers all three GoF categories: creational, structural and behavioural.
    /// </summary>
    public class PatternsTests
    {
        // ── Creational: Factory ──────────────────────────────────────────────
        [Theory]
        [InlineData(UserRole.Student, typeof(Student))]
        [InlineData(UserRole.Faculty, typeof(Faculty))]
        [InlineData(UserRole.Admin, typeof(Admin))]
        [InlineData(UserRole.Parent, typeof(Parent))]
        public void UserFactory_CreatesConcreteTypeForRole(UserRole role, Type expectedType)
        {
            var user = UserFactory.Create(role, "Test User", "test@sims.edu", "hash");

            Assert.IsType(expectedType, user);
            Assert.Equal(role, user.Role);
            Assert.Equal("test@sims.edu", user.Email);
        }

        [Fact]
        public void UserFactory_UnknownRole_Throws()
        {
            Assert.Throws<ArgumentException>(() => UserFactory.Create((UserRole)999, "X", "x@sims.edu", "hash"));
        }

        // ── Creational: Singleton ────────────────────────────────────────────
        [Fact]
        public void SystemConfiguration_AlwaysReturnsTheSameInstance()
        {
            Assert.Same(SystemConfiguration.Instance, SystemConfiguration.Instance);
        }

        // ── Creational: Builder ──────────────────────────────────────────────
        [Fact]
        public void ImportOptionsBuilder_AppliesEveryConfiguredValue()
        {
            var options = new ImportOptionsBuilder()
                .WithBatchSize(250)
                .StopOnFirstError()
                .ValidateOnly()
                .UpdateExisting()
                .WithMaxReportedErrors(10)
                .Build();

            Assert.Equal(250, options.BatchSize);
            Assert.True(options.StopOnFirstError);
            Assert.True(options.ValidateOnly);
            Assert.True(options.UpdateExisting);
            Assert.Equal(10, options.MaxReportedErrors);
        }

        [Fact]
        public void ImportOptionsBuilder_Defaults_AreSafe()
        {
            var options = ImportOptions.Default;

            Assert.Equal(1_000, options.BatchSize);
            Assert.False(options.StopOnFirstError);
            Assert.False(options.ValidateOnly);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void ImportOptionsBuilder_InvalidBatchSize_Throws(int batchSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ImportOptionsBuilder().WithBatchSize(batchSize));
        }

        // ── Behavioural: Strategy ────────────────────────────────────────────
        [Theory]
        [InlineData(GradingScheme.Letter, typeof(LetterGradeStrategy))]
        [InlineData(GradingScheme.Btec, typeof(BtecGradeStrategy))]
        [InlineData(GradingScheme.Numeric, typeof(NumericGradeStrategy))]
        public void GradeStrategyFactory_ReturnsStrategyForScheme(GradingScheme scheme, Type expectedType)
        {
            Assert.IsType(expectedType, GradeStrategyFactory.Create(scheme));
        }

        /// <summary>
        /// Liskov Substitution check: every strategy must interpret a grade on the same 0–10 scale so
        /// the caller can swap one for another without special-case handling.
        /// <para>
        /// This test originally failed and exposed a real defect — <c>LetterGradeStrategy</c> was
        /// written against a 0–100 scale, so selecting the Letter scheme marked every passing student
        /// as failing. It now guards against that regression.
        /// </para>
        /// </summary>
        [Fact]
        public void AllGradeStrategies_HonourTheSameContract()
        {
            IGradeStrategy[] strategies =
            {
                new LetterGradeStrategy(),
                new BtecGradeStrategy(),
                new NumericGradeStrategy()
            };

            foreach (var strategy in strategies)
            {
                Assert.False(string.IsNullOrWhiteSpace(strategy.StrategyName));
                Assert.False(string.IsNullOrWhiteSpace(strategy.CalculateLetterGrade(8.0)));
                Assert.False(string.IsNullOrWhiteSpace(strategy.GetGradeDescription(8.0)));
                Assert.False(string.IsNullOrWhiteSpace(strategy.GetGradePoint(8.0)));

                Assert.True(strategy.IsPassing(9.0), $"{strategy.StrategyName} should treat 9.0/10 as a pass.");
                Assert.False(strategy.IsPassing(1.0), $"{strategy.StrategyName} should treat 1.0/10 as a fail.");
            }
        }

        /// <summary>
        /// All strategies must agree on the pass/fail verdict at the extremes of the 0–10 scale,
        /// even though they use different labels. This is the substitutability guarantee callers rely on.
        /// </summary>
        [Theory]
        [InlineData(10.0, true)]
        [InlineData(9.5, true)]
        [InlineData(0.0, false)]
        [InlineData(2.0, false)]
        public void AllGradeStrategies_AgreeOnPassFailAtTheExtremes(double grade, bool expectedPassing)
        {
            IGradeStrategy[] strategies =
            {
                new LetterGradeStrategy(),
                new BtecGradeStrategy(),
                new NumericGradeStrategy()
            };

            Assert.All(strategies, s => Assert.Equal(expectedPassing, s.IsPassing(grade)));
        }

        // ── Behavioural: Observer ────────────────────────────────────────────
        /// <summary>
        /// Uses Moq (a vendor-provided mocking library) to verify the publisher notifies every
        /// subscriber. Verifying an interaction like this is far easier with a mock than with a
        /// hand-written stub, which is precisely the trade-off discussed in the report.
        /// </summary>
        [Fact]
        public void EnrollmentEventPublisher_NotifiesAllSubscribers()
        {
            var firstObserver = new Mock<IEnrollmentObserver>();
            var secondObserver = new Mock<IEnrollmentObserver>();

            var publisher = new EnrollmentEventPublisher();
            publisher.Subscribe(firstObserver.Object);
            publisher.Subscribe(secondObserver.Object);

            var student = TestData.CreateStudent();
            var course = TestData.CreateCourse();

            publisher.NotifyEnrolled(student, course);

            firstObserver.Verify(o => o.OnStudentEnrolled(student, course), Times.Once);
            secondObserver.Verify(o => o.OnStudentEnrolled(student, course), Times.Once);
        }

        [Fact]
        public void EnrollmentEventPublisher_AfterUnsubscribe_ObserverIsNotNotified()
        {
            var observer = new Mock<IEnrollmentObserver>();
            var publisher = new EnrollmentEventPublisher();

            publisher.Subscribe(observer.Object);
            publisher.Unsubscribe(observer.Object);
            publisher.NotifyDropped(TestData.CreateStudent(), TestData.CreateCourse());

            observer.Verify(o => o.OnStudentDropped(It.IsAny<Student>(), It.IsAny<Course>()), Times.Never);
        }

        [Fact]
        public void EnrollmentEventPublisher_SubscribingTwice_DoesNotDuplicateNotifications()
        {
            var observer = new Mock<IEnrollmentObserver>();
            var publisher = new EnrollmentEventPublisher();

            publisher.Subscribe(observer.Object);
            publisher.Subscribe(observer.Object);
            publisher.NotifyGradeUpdated(TestData.CreateStudent(), TestData.CreateCourse(), 8.5);

            observer.Verify(o => o.OnGradeUpdated(It.IsAny<Student>(), It.IsAny<Course>(), 8.5), Times.Once);
        }

        // ── Structural: Adapter ──────────────────────────────────────────────
        [Fact]
        public void StudentCsvMapper_MapsRecordToEntity()
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["StudentCode"] = "BH00042",
                ["FullName"] = "Tran Thi B",
                ["Email"] = "TRAN.B@SIMS.EDU",
                ["DateOfBirth"] = "2003-11-02",
                ["Gender"] = "Female",
                ["Program"] = "Data Science",
                ["Department"] = "Computing",
                ["EnrollmentYear"] = "2023",
                ["GPA"] = "8.75",
                ["AcademicStatus"] = "Active"
            };

            var student = new StudentCsvMapper().ToEntity(new CsvRecord(2, fields.Values.ToList(), fields), "hash");

            Assert.Equal("BH00042", student.StudentCode);
            Assert.Equal("tran.b@sims.edu", student.Email);       // Normalised to lower case.
            Assert.Equal(new DateTime(2003, 11, 2), student.DateOfBirth);
            Assert.Equal(8.75, student.GPA);
            Assert.Equal(2023, student.EnrollmentYear);
            Assert.Equal(AcademicStatus.Active, student.AcademicStatus);
        }

        [Fact]
        public void StudentCsvMapper_MissingOptionalFields_FallsBackToDefaults()
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FullName"] = "Le Van C",
                ["Email"] = "le.c@sims.edu",
                ["Program"] = "Computer Science"
            };

            var student = new StudentCsvMapper().ToEntity(new CsvRecord(2, fields.Values.ToList(), fields), "hash");

            Assert.Equal("General", student.Department);
            Assert.Equal("Unspecified", student.Gender);
            Assert.Equal(0.0, student.GPA);
            Assert.Equal(AcademicStatus.Active, student.AcademicStatus);
        }

        [Fact]
        public void StudentCsvMapper_ToRow_MatchesColumnOrder()
        {
            var row = new StudentCsvMapper().ToRow(TestData.CreateStudent());

            Assert.Equal(StudentCsvMapper.Columns.Count, row.Count);
            Assert.Equal("BH00001", row[0]);
            Assert.Equal("Nguyen Van A", row[1]);
            Assert.Equal("nguyen.van.a@sims.edu", row[2]);
        }

        // ── Supporting type: PagedResult ─────────────────────────────────────
        [Theory]
        [InlineData(0, 20, 0)]
        [InlineData(100, 20, 5)]
        [InlineData(101, 20, 6)]
        [InlineData(19, 20, 1)]
        public void PagedResult_TotalPages_RoundsUp(int totalCount, int pageSize, int expectedPages)
        {
            var page = new PagedResult<string>(Array.Empty<string>(), 1, pageSize, totalCount);

            Assert.Equal(expectedPages, page.TotalPages);
        }

        [Fact]
        public void PagedResult_NavigationFlags_ReflectPosition()
        {
            var middlePage = new PagedResult<string>(new[] { "a" }, pageNumber: 2, pageSize: 10, totalCount: 30);

            Assert.True(middlePage.HasPreviousPage);
            Assert.True(middlePage.HasNextPage);

            var lastPage = new PagedResult<string>(new[] { "a" }, pageNumber: 3, pageSize: 10, totalCount: 30);

            Assert.True(lastPage.HasPreviousPage);
            Assert.False(lastPage.HasNextPage);
        }
    }
}
