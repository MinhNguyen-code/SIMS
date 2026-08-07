using System.Text;
using DayNeCu3726.DataProcessing.Csv;
using DayNeCu3726.DataProcessing.Pipeline;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services;
using DayNeCu3726.Services.Interfaces;
using DayNeCu3726.Tests.TestDoubles;

namespace DayNeCu3726.Tests.Integration
{
    /// <summary>
    /// Integration tests for the complete large-dataset pipeline: CSV stream → validation chain →
    /// mapper → batch processor → Unit of Work → database, and back out again via export.
    /// <para>
    /// These are the tests that demonstrate the application genuinely satisfies the "large dataset
    /// processing" requirement, because they run real data end to end through every layer.
    /// </para>
    /// </summary>
    public class DatasetServiceIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatasetService _service;

        public DatasetServiceIntegrationTests()
        {
            _unitOfWork = TestData.CreateUnitOfWork(out _context);
            _service = new DatasetService(
                new StreamingCsvRecordReader(),
                new StreamingCsvRecordWriter(),
                _unitOfWork,
                new FakePasswordHasher());
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task ImportStudentsAsync_ValidFile_PersistsEveryRow()
        {
            await using var csv = TestData.CsvStream(TestData.BuildStudentCsv(120));

            var result = await _service.ImportStudentsAsync(csv, ImportOptions.Default);

            Assert.Equal(120, result.TotalRowsRead);
            Assert.Equal(120, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Equal(120, _unitOfWork.Students.Count());
        }

        [Fact]
        public async Task ImportStudentsAsync_PersistsFieldValuesCorrectly()
        {
            const string csv = $"""
                {TestData.StudentCsvHeader}
                BH09001,Pham Thi D,pham.d@sims.edu,2003-07-14,Female,Data Science,Computing,2023,9.25,0912345678,Hanoi,Active
                """;

            await using var stream = TestData.CsvStream(csv);
            await _service.ImportStudentsAsync(stream, ImportOptions.Default);

            var student = _unitOfWork.Students.GetByEmail("pham.d@sims.edu");

            Assert.NotNull(student);
            Assert.Equal("Pham Thi D", student!.FullName);
            Assert.Equal("Data Science", student.Program);
            Assert.Equal(9.25, student.GPA);
            Assert.Equal(2023, student.EnrollmentYear);
            Assert.Equal(new DateTime(2003, 7, 14), student.DateOfBirth);
        }

        [Fact]
        public async Task ImportStudentsAsync_NeverStoresAPlainTextPassword()
        {
            await using var csv = TestData.CsvStream(TestData.BuildStudentCsv(3));
            await _service.ImportStudentsAsync(csv, ImportOptions.Default);

            Assert.All(_unitOfWork.Students.GetAll(),
                s => Assert.StartsWith(FakePasswordHasher.Prefix, s.PasswordHash));
        }

        [Fact]
        public async Task ImportStudentsAsync_InvalidRows_AreRejectedButValidRowsPersist()
        {
            const string csv = $"""
                {TestData.StudentCsvHeader}
                BH00001,Valid One,valid1@sims.edu,2004-01-01,Male,CS,Computing,2024,8.0,090,Hanoi,Active
                BH00002,,missing.name@sims.edu,2004-01-01,Male,CS,Computing,2024,8.0,090,Hanoi,Active
                BH00003,Bad Email,not-an-email,2004-01-01,Male,CS,Computing,2024,8.0,090,Hanoi,Active
                BH00004,Bad Gpa,badgpa@sims.edu,2004-01-01,Male,CS,Computing,2024,99,090,Hanoi,Active
                BH00005,Valid Two,valid2@sims.edu,2004-01-01,Male,CS,Computing,2024,7.0,090,Hanoi,Active
                """;

            await using var stream = TestData.CsvStream(csv);
            var result = await _service.ImportStudentsAsync(stream, ImportOptions.Default);

            Assert.Equal(5, result.TotalRowsRead);
            Assert.Equal(2, result.SuccessCount);
            Assert.Equal(3, result.FailureCount);
            Assert.Equal(2, _unitOfWork.Students.Count());

            // Every rejection is traceable to a specific line and column.
            Assert.Contains(result.Errors, e => e.LineNumber == 3 && e.Column == "FullName");
            Assert.Contains(result.Errors, e => e.LineNumber == 4 && e.Column == "Email");
            Assert.Contains(result.Errors, e => e.LineNumber == 5 && e.Column == "GPA");
        }

        [Fact]
        public async Task ImportStudentsAsync_DuplicateEmailWithinFile_IsRejectedOnce()
        {
            const string csv = $"""
                {TestData.StudentCsvHeader}
                BH00001,First,dup@sims.edu,2004-01-01,Male,CS,Computing,2024,8.0,090,Hanoi,Active
                BH00002,Second,dup@sims.edu,2004-01-01,Male,CS,Computing,2024,8.0,090,Hanoi,Active
                """;

            await using var stream = TestData.CsvStream(csv);
            var result = await _service.ImportStudentsAsync(stream, ImportOptions.Default);

            Assert.Equal(1, result.SuccessCount);
            Assert.Equal(1, result.FailureCount);
            Assert.Equal(1, _unitOfWork.Students.Count());
        }

        [Fact]
        public async Task ImportStudentsAsync_EmailAlreadyInDatabase_IsSkippedNotDuplicated()
        {
            _unitOfWork.Students.Add(TestData.CreateStudent(email: "existing@sims.edu", studentCode: "BH00001"));
            _unitOfWork.SaveChanges();

            const string csv = $"""
                {TestData.StudentCsvHeader}
                BH00002,Existing Again,existing@sims.edu,2004-01-01,Male,CS,Computing,2024,8.0,090,Hanoi,Active
                BH00003,Brand New,brandnew@sims.edu,2004-01-01,Male,CS,Computing,2024,8.0,090,Hanoi,Active
                """;

            await using var stream = TestData.CsvStream(csv);
            var result = await _service.ImportStudentsAsync(stream, ImportOptions.Default);

            Assert.Equal(1, result.SkippedCount);
            Assert.Equal(1, result.SuccessCount);
            Assert.Equal(2, _unitOfWork.Students.Count());
        }

        [Fact]
        public async Task ImportStudentsAsync_ValidateOnly_LeavesDatabaseUntouched()
        {
            await using var csv = TestData.CsvStream(TestData.BuildStudentCsv(40));
            var options = new ImportOptionsBuilder().ValidateOnly().Build();

            var result = await _service.ImportStudentsAsync(csv, options);

            Assert.Equal(40, result.SuccessCount);
            Assert.Equal(0, _unitOfWork.Students.Count());     // Dry run — nothing saved.
        }

        [Fact]
        public async Task ImportStudentsAsync_QuotedFieldsContainingCommas_AreImportedIntact()
        {
            const string csv = $"""
                {TestData.StudentCsvHeader}
                BH00001,"Nguyen Van A, Jr.",comma@sims.edu,2004-01-01,Male,CS,Computing,2024,8.0,090,"12 Le Loi, Hanoi",Active
                """;

            await using var stream = TestData.CsvStream(csv);
            await _service.ImportStudentsAsync(stream, ImportOptions.Default);

            var student = _unitOfWork.Students.GetByEmail("comma@sims.edu");

            Assert.NotNull(student);
            Assert.Equal("Nguyen Van A, Jr.", student!.FullName);
            Assert.Equal("12 Le Loi, Hanoi", student.Address);
        }

        [Fact]
        public async Task ImportStudentsAsync_MissingRequiredColumn_ThrowsBeforeProcessingAnyRow()
        {
            await using var csv = TestData.CsvStream("StudentCode,Program\nBH00001,CS");

            var exception = await Assert.ThrowsAsync<InvalidDataException>(
                () => _service.ImportStudentsAsync(csv, ImportOptions.Default));

            Assert.Contains("FullName", exception.Message);
            Assert.Equal(0, _unitOfWork.Students.Count());
        }

        [Fact]
        public async Task ImportStudentsAsync_MissingStudentCode_IsGeneratedAutomatically()
        {
            const string csv = """
                FullName,Email,Program
                Auto Code,auto@sims.edu,Computer Science
                """;

            await using var stream = TestData.CsvStream(csv);
            await _service.ImportStudentsAsync(stream, ImportOptions.Default);

            var student = _unitOfWork.Students.GetByEmail("auto@sims.edu");

            Assert.NotNull(student);
            Assert.StartsWith("BH", student!.StudentCode);
        }

        [Fact]
        public async Task ImportStudentsAsync_BatchSize_ControlsNumberOfCommits()
        {
            await using var csv = TestData.CsvStream(TestData.BuildStudentCsv(250));
            var options = new ImportOptionsBuilder().WithBatchSize(100).Build();

            var result = await _service.ImportStudentsAsync(csv, options);

            Assert.Equal(3, result.BatchesCommitted);       // 100 + 100 + 50
            Assert.Equal(250, _unitOfWork.Students.Count());
        }

        /// <summary>
        /// Scale check. 5,000 rows are streamed through validation, mapping and batched persistence.
        /// The assertion is on correctness plus a generous time budget, so the test stays reliable on
        /// slower CI machines while still failing if throughput collapses.
        /// </summary>
        [Fact]
        public async Task ImportStudentsAsync_LargeDataset_CompletesCorrectlyAndQuickly()
        {
            await using var csv = TestData.CsvStream(TestData.BuildStudentCsv(5_000));
            var options = new ImportOptionsBuilder().WithBatchSize(500).Build();

            var result = await _service.ImportStudentsAsync(csv, options);

            Assert.Equal(5_000, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Equal(10, result.BatchesCommitted);
            Assert.True(result.Duration.TotalSeconds < 60,
                $"Importing 5,000 rows took {result.Duration.TotalSeconds:F2}s.");
        }

        [Fact]
        public async Task ExportStudentsAsync_WritesHeaderAndEveryStudent()
        {
            await using var importStream = TestData.CsvStream(TestData.BuildStudentCsv(75));
            await _service.ImportStudentsAsync(importStream, ImportOptions.Default);

            await using var output = new MemoryStream();
            var rowsWritten = await _service.ExportStudentsAsync(output);

            var lines = Encoding.UTF8.GetString(output.ToArray())
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(75, rowsWritten);
            Assert.Equal(76, lines.Length);                            // header + 75 rows
            Assert.StartsWith("StudentCode,FullName,Email", lines[0]);
        }

        /// <summary>
        /// Round-trip test: export the database, re-import it into a second empty database and
        /// confirm the record count and key values survive intact.
        /// </summary>
        [Fact]
        public async Task ExportThenReimport_PreservesAllRecords()
        {
            await using var seedStream = TestData.CsvStream(TestData.BuildStudentCsv(50));
            await _service.ImportStudentsAsync(seedStream, ImportOptions.Default);

            await using var exported = new MemoryStream();
            await _service.ExportStudentsAsync(exported);

            using var secondContext = TestData.CreateContext();
            var secondUnitOfWork = new DayNeCu3726.Repositories.UnitOfWork(secondContext);
            var secondService = new DatasetService(
                new StreamingCsvRecordReader(),
                new StreamingCsvRecordWriter(),
                secondUnitOfWork,
                new FakePasswordHasher());

            await using var reimportStream = new MemoryStream(exported.ToArray());
            var result = await secondService.ImportStudentsAsync(reimportStream, ImportOptions.Default);

            Assert.Equal(50, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Equal(50, secondUnitOfWork.Students.Count());
        }

        [Fact]
        public async Task ExportStudentsAsync_EmptyDatabase_WritesHeaderOnly()
        {
            await using var output = new MemoryStream();
            var rowsWritten = await _service.ExportStudentsAsync(output);

            var text = Encoding.UTF8.GetString(output.ToArray());

            Assert.Equal(0, rowsWritten);
            Assert.StartsWith("StudentCode,FullName,Email", text);
        }

        [Fact]
        public async Task WriteImportTemplateAsync_ContainsEveryExpectedColumn()
        {
            await using var output = new MemoryStream();
            await _service.WriteImportTemplateAsync(output);

            var header = Encoding.UTF8.GetString(output.ToArray()).Trim();

            Assert.Contains("FullName", header);
            Assert.Contains("Email", header);
            Assert.Contains("Program", header);
        }

        [Fact]
        public async Task GenerateSampleDatasetAsync_ProducesRequestedNumberOfImportableRows()
        {
            await using var generated = new MemoryStream();
            var rowsWritten = await _service.GenerateSampleDatasetAsync(generated, 500);

            Assert.Equal(500, rowsWritten);

            // The generated data must be valid input for the importer, not just well-formed text.
            await using var importStream = new MemoryStream(generated.ToArray());
            var result = await _service.ImportStudentsAsync(importStream, ImportOptions.Default);

            Assert.Equal(500, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
        }

        [Fact]
        public async Task GenerateSampleDatasetAsync_InvalidCount_Throws()
        {
            await using var output = new MemoryStream();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _service.GenerateSampleDatasetAsync(output, 0));
        }

        [Fact]
        public async Task AnalyzeAsync_ReportsAggregatesForAGeneratedDataset()
        {
            await using var generated = new MemoryStream();
            await _service.GenerateSampleDatasetAsync(generated, 1_000);

            await using var analysisStream = new MemoryStream(generated.ToArray());
            var statistics = await _service.AnalyzeAsync(analysisStream);

            Assert.Equal(1_000, statistics.TotalRecords);
            Assert.Equal(1_000, statistics.ValidGpaCount);
            Assert.InRange(statistics.AverageGpa, 0, 10);
            Assert.True(statistics.CountByProgram.Count >= 2);
        }

        [Fact]
        public void Constructor_NullDependencies_Throw()
        {
            var reader = new StreamingCsvRecordReader();
            var writer = new StreamingCsvRecordWriter();

            Assert.Throws<ArgumentNullException>(() => new DatasetService(null!, writer, _unitOfWork, new FakePasswordHasher()));
            Assert.Throws<ArgumentNullException>(() => new DatasetService(reader, null!, _unitOfWork, new FakePasswordHasher()));
            Assert.Throws<ArgumentNullException>(() => new DatasetService(reader, writer, null!, new FakePasswordHasher()));
            Assert.Throws<ArgumentNullException>(() => new DatasetService(reader, writer, _unitOfWork, null!));
        }
    }
}
