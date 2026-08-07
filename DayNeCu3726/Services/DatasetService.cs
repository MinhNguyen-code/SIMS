using System.Globalization;
using System.Runtime.CompilerServices;
using DayNeCu3726.DataProcessing.Abstractions;
using DayNeCu3726.DataProcessing.Mapping;
using DayNeCu3726.DataProcessing.Pipeline;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Security;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    /// <summary>
    /// Coordinates the large-dataset CSV workflows: import, export, analysis and sample generation.
    /// <para>
    /// Every collaborator is injected as an abstraction (<see cref="ICsvRecordReader"/>,
    /// <see cref="ICsvRecordWriter"/>, <see cref="IUnitOfWork"/>, <see cref="IPasswordHasher"/>),
    /// so the whole class is unit-testable with in-memory doubles — the Dependency Inversion
    /// Principle turned directly into testability.
    /// </para>
    /// </summary>
    public sealed class DatasetService : IDatasetService
    {
        private const int ExportPageSize = 2_000;

        private readonly ICsvRecordReader _reader;
        private readonly ICsvRecordWriter _writer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly StudentCsvMapper _mapper = new();

        public DatasetService(
            ICsvRecordReader reader,
            ICsvRecordWriter writer,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public Task<ImportResult> ImportStudentsAsync(Stream csvSource, ImportOptions options, CancellationToken cancellationToken = default)
        {
            var processor = new StudentCsvImportProcessor(_reader, _unitOfWork, _passwordHasher);
            return processor.ExecuteAsync(csvSource, options, cancellationToken);
        }

        public Task<int> ExportStudentsAsync(Stream destination, CancellationToken cancellationToken = default) =>
            _writer.WriteRowsAsync(destination, StudentCsvMapper.Columns, StreamStudentRowsAsync(cancellationToken), cancellationToken);

        public Task<DatasetStatistics> AnalyzeAsync(Stream csvSource, CancellationToken cancellationToken = default) =>
            new CsvDatasetAnalyzer(_reader).AnalyzeAsync(csvSource, cancellationToken);

        public Task WriteImportTemplateAsync(Stream destination, CancellationToken cancellationToken = default) =>
            _writer.WriteHeaderAsync(destination, StudentCsvMapper.Columns, cancellationToken);

        public Task<int> GenerateSampleDatasetAsync(Stream destination, int recordCount, CancellationToken cancellationToken = default)
        {
            if (recordCount < 1)
                throw new ArgumentOutOfRangeException(nameof(recordCount), "Record count must be at least 1.");

            return _writer.WriteRowsAsync(destination, StudentCsvMapper.Columns, GenerateRowsAsync(recordCount, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Yields students page by page so the export never holds the full table in memory.
        /// This is the difference between exporting 200 000 students successfully and running out of memory.
        /// </summary>
        private async IAsyncEnumerable<IReadOnlyList<string>> StreamStudentRowsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var pageNumber = 1;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var page = _unitOfWork.Students.GetPaged(pageNumber, ExportPageSize);
                if (page.Items.Count == 0)
                    yield break;

                foreach (var student in page.Items)
                    yield return _mapper.ToRow(student);

                if (!page.HasNextPage)
                    yield break;

                pageNumber++;
                await Task.Yield();     // Keeps the request responsive between pages.
            }
        }

        /// <summary>Produces deterministic synthetic rows for benchmarking and demonstrations.</summary>
        private static async IAsyncEnumerable<IReadOnlyList<string>> GenerateRowsAsync(
            int recordCount,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string[] programs = { "Computer Science", "Software Engineering", "Information Systems", "Data Science", "Cyber Security" };
            string[] departments = { "Computing", "Engineering", "Business", "Science" };
            string[] genders = { "Male", "Female", "Unspecified" };

            // A fixed seed makes generated datasets reproducible, so benchmark runs are comparable.
            var random = new Random(3726);

            for (var i = 1; i <= recordCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var enrollmentYear = 2018 + (i % 8);
                var gpa = Math.Round(random.NextDouble() * 10, 2);

                yield return new[]
                {
                    $"BH{i:D6}",
                    $"Sample Student {i}",
                    $"sample.student{i}@sims.edu",
                    new DateTime(2000 + (i % 8), (i % 12) + 1, (i % 28) + 1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    genders[i % genders.Length],
                    programs[i % programs.Length],
                    departments[i % departments.Length],
                    enrollmentYear.ToString(CultureInfo.InvariantCulture),
                    gpa.ToString("F2", CultureInfo.InvariantCulture),
                    $"09{i % 100000000:D8}",
                    $"{i} Sample Street, Hanoi",
                    "Active"
                };

                if (i % 5_000 == 0)
                    await Task.Yield();
            }
        }
    }
}
