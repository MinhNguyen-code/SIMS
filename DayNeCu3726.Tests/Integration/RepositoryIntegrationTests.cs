using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Tests.TestDoubles;

namespace DayNeCu3726.Tests.Integration
{
    /// <summary>
    /// Integration tests for the repository and Unit of Work layer.
    /// <para>
    /// Unlike the unit tests, these exercise several components together — repository, Unit of Work,
    /// entity configuration and the EF Core provider — against a real (in-memory) database. They
    /// catch the class of defect a unit test cannot: mapping mistakes, broken queries and
    /// transaction boundaries that only appear once the pieces are combined.
    /// </para>
    /// </summary>
    public class RepositoryIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public RepositoryIntegrationTests()
        {
            _unitOfWork = TestData.CreateUnitOfWork(out _context);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void AddThenSave_PersistsStudentAcrossQueries()
        {
            var student = TestData.CreateStudent();

            _unitOfWork.Students.Add(student);
            _unitOfWork.SaveChanges();

            var reloaded = _unitOfWork.Students.GetById(student.Id);

            Assert.NotNull(reloaded);
            Assert.Equal("Nguyen Van A", reloaded!.FullName);
        }

        [Fact]
        public void GetByEmail_IsCaseInsensitive()
        {
            _unitOfWork.Students.Add(TestData.CreateStudent(email: "case.test@sims.edu"));
            _unitOfWork.SaveChanges();

            Assert.NotNull(_unitOfWork.Students.GetByEmail("CASE.TEST@SIMS.EDU"));
        }

        [Fact]
        public void GetByStudentCode_ReturnsMatchingStudent()
        {
            _unitOfWork.Students.Add(TestData.CreateStudent(studentCode: "BH12345"));
            _unitOfWork.SaveChanges();

            var found = _unitOfWork.Students.GetByStudentCode("BH12345");

            Assert.NotNull(found);
            Assert.Equal("BH12345", found!.StudentCode);
        }

        /// <summary>
        /// Verifies the paging behaviour introduced for the Scalability requirement: only the
        /// requested slice is returned while the total count reflects the whole dataset.
        /// </summary>
        [Fact]
        public void GetPaged_ReturnsRequestedSliceAndCorrectTotals()
        {
            SeedStudents(55);

            var page = _unitOfWork.Students.GetPaged(pageNumber: 2, pageSize: 20);

            Assert.Equal(20, page.Items.Count);
            Assert.Equal(55, page.TotalCount);
            Assert.Equal(3, page.TotalPages);
            Assert.True(page.HasPreviousPage);
            Assert.True(page.HasNextPage);
        }

        [Fact]
        public void GetPaged_LastPage_ReturnsRemainderOnly()
        {
            SeedStudents(55);

            var page = _unitOfWork.Students.GetPaged(pageNumber: 3, pageSize: 20);

            Assert.Equal(15, page.Items.Count);
            Assert.False(page.HasNextPage);
        }

        [Fact]
        public void GetPaged_WithPredicate_FiltersBeforePaging()
        {
            SeedStudents(30, program: "Computer Science");
            SeedStudents(10, program: "Data Science", startIndex: 100);

            var page = _unitOfWork.Students.GetPaged(1, 50, s => s.Program == "Data Science");

            Assert.Equal(10, page.TotalCount);
            Assert.All(page.Items, s => Assert.Equal("Data Science", s.Program));
        }

        [Theory]
        [InlineData(0, 20)]
        [InlineData(-3, 20)]
        [InlineData(1, 0)]
        public void GetPaged_InvalidArguments_AreClampedInsteadOfThrowing(int pageNumber, int pageSize)
        {
            SeedStudents(5);

            var page = _unitOfWork.Students.GetPaged(pageNumber, pageSize);

            Assert.True(page.PageNumber >= 1);
            Assert.True(page.PageSize >= 1);
        }

        [Fact]
        public void Query_WithExpression_FiltersServerSide()
        {
            SeedStudents(10, program: "Computer Science");
            SeedStudents(5, program: "Cyber Security", startIndex: 100);

            var results = _unitOfWork.Students.Query(s => s.Program == "Cyber Security").ToList();

            Assert.Equal(5, results.Count);
        }

        [Fact]
        public void CountWithPredicate_CountsOnlyMatchingRows()
        {
            SeedStudents(8, program: "Computer Science");
            SeedStudents(4, program: "Data Science", startIndex: 100);

            Assert.Equal(4, _unitOfWork.Students.Count(s => s.Program == "Data Science"));
            Assert.Equal(12, _unitOfWork.Students.Count());
        }

        [Fact]
        public void AddRange_InsertsEveryEntityInOneCall()
        {
            var students = Enumerable.Range(1, 25)
                .Select(i => TestData.CreateStudent(email: $"bulk{i}@sims.edu", studentCode: $"BH{i:D5}"))
                .ToList();

            _unitOfWork.Students.AddRange(students);
            _unitOfWork.SaveChanges();

            Assert.Equal(25, _unitOfWork.Students.Count());
        }

        [Fact]
        public void Update_PersistsChangedValues()
        {
            var student = TestData.CreateStudent();
            _unitOfWork.Students.Add(student);
            _unitOfWork.SaveChanges();

            student.AcademicStatus = AcademicStatus.Suspended;
            _unitOfWork.Students.Update(student);
            _unitOfWork.SaveChanges();

            Assert.Equal(AcademicStatus.Suspended, _unitOfWork.Students.GetById(student.Id)!.AcademicStatus);
        }

        [Fact]
        public void Delete_RemovesEntity()
        {
            var student = TestData.CreateStudent();
            _unitOfWork.Students.Add(student);
            _unitOfWork.SaveChanges();

            _unitOfWork.Students.Delete(student.Id);
            _unitOfWork.SaveChanges();

            Assert.False(_unitOfWork.Students.Exists(student.Id));
        }

        /// <summary>
        /// Regression test for the duplicate-code defect: the old implementation derived the next
        /// code from <c>Count() + 1</c>, so deleting a student caused the next registration to reuse
        /// a code that already existed.
        /// </summary>
        [Fact]
        public void GenerateStudentCode_AfterDeletion_DoesNotReuseAnExistingCode()
        {
            var first = TestData.CreateStudent(email: "one@sims.edu", studentCode: "BH00001");
            var second = TestData.CreateStudent(email: "two@sims.edu", studentCode: "BH00002");

            _unitOfWork.Students.Add(first);
            _unitOfWork.Students.Add(second);
            _unitOfWork.SaveChanges();

            _unitOfWork.Students.Delete(first.Id);
            _unitOfWork.SaveChanges();

            var nextCode = _unitOfWork.Students.GenerateStudentCode(2025);

            Assert.NotEqual("BH00002", nextCode);
            Assert.Equal("BH00003", nextCode);
        }

        [Fact]
        public void GenerateStudentCode_OnEmptyDatabase_StartsAtOne()
        {
            Assert.Equal("BH00001", _unitOfWork.Students.GenerateStudentCode(2025));
        }

        [Fact]
        public void GenerateStudentCode_ProducesUniqueSequentialCodes()
        {
            var generated = new HashSet<string>();

            for (var i = 0; i < 10; i++)
            {
                var code = _unitOfWork.Students.GenerateStudentCode(2025);
                Assert.True(generated.Add(code), $"Code {code} was generated twice.");

                _unitOfWork.Students.Add(TestData.CreateStudent(email: $"seq{i}@sims.edu", studentCode: code));
                _unitOfWork.SaveChanges();
            }

            Assert.Equal(10, generated.Count);
        }

        private void SeedStudents(int count, string program = "Computer Science", int startIndex = 0)
        {
            for (var i = startIndex + 1; i <= startIndex + count; i++)
            {
                _unitOfWork.Students.Add(TestData.CreateStudent(
                    fullName: $"Student {i}",
                    email: $"student{i}@sims.edu",
                    program: program,
                    studentCode: $"BH{i:D5}"));
            }

            _unitOfWork.SaveChanges();
        }
    }
}
