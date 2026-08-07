using DayNeCu3726.DataProcessing.Abstractions;
using DayNeCu3726.DataProcessing.Csv;
using DayNeCu3726.DataProcessing.Pipeline;
using DayNeCu3726.DataProcessing.Validation;
using DayNeCu3726.Tests.TestDoubles;

namespace DayNeCu3726.Tests.Unit
{
    /// <summary>
    /// Unit tests for the Template Method import pipeline.
    /// <para>
    /// A minimal test-only subclass is used so the batching, validation and error-reporting skeleton
    /// can be verified without touching a database. That the base class is testable in isolation is
    /// itself evidence that the responsibilities were separated correctly.
    /// </para>
    /// </summary>
    public class BatchImportProcessorTests
    {
        /// <summary>Test double subclass that records the batches it is asked to persist.</summary>
        private sealed class RecordingProcessor : BatchImportProcessor<string>
        {
            public List<List<string>> PersistedBatches { get; } = new();
            public int BatchCommittedCallbacks { get; private set; }

            public RecordingProcessor(ICsvRecordReader reader) : base(reader) { }

            protected override IReadOnlyList<string> RequiredColumns => new[] { "Name" };

            protected override RowValidationHandler BuildValidationChain()
            {
                var head = new RequiredColumnsHandler("Name");
                head.SetNext(new NumericRangeHandler("Score", 0, 100, optional: true));
                return head;
            }

            protected override string? MapRecord(CsvRecord record, ImportOptions options)
            {
                if (record["Name"].Equals("SKIP", StringComparison.OrdinalIgnoreCase))
                    return null;

                if (record["Name"].Equals("THROW", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Mapping deliberately failed.");

                return record["Name"];
            }

            protected override Task PersistBatchAsync(IReadOnlyList<string> batch, ImportOptions options, CancellationToken cancellationToken)
            {
                PersistedBatches.Add(batch.ToList());
                return Task.CompletedTask;
            }

            protected override Task OnBatchCommittedAsync(int batchNumber, int rowsProcessed, CancellationToken cancellationToken)
            {
                BatchCommittedCallbacks++;
                return Task.CompletedTask;
            }
        }

        private static RecordingProcessor CreateProcessor() => new(new StreamingCsvRecordReader());

        [Fact]
        public async Task ExecuteAsync_ValidRows_AreAllImported()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream("Name,Score\nAlpha,50\nBeta,60\nGamma,70");

            var result = await processor.ExecuteAsync(stream);

            Assert.Equal(3, result.TotalRowsRead);
            Assert.Equal(3, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.True(result.IsSuccess);
        }

        /// <summary>Batching is the core performance behaviour of the pipeline, so it is asserted directly.</summary>
        [Fact]
        public async Task ExecuteAsync_SplitsRowsIntoBatchesOfTheConfiguredSize()
        {
            var processor = CreateProcessor();
            var csv = "Name\n" + string.Join("\n", Enumerable.Range(1, 10).Select(i => $"Row{i}"));
            await using var stream = TestData.CsvStream(csv);

            var options = new ImportOptionsBuilder().WithBatchSize(4).Build();
            var result = await processor.ExecuteAsync(stream, options);

            Assert.Equal(3, result.BatchesCommitted);                       // 4 + 4 + 2
            Assert.Equal(new[] { 4, 4, 2 }, processor.PersistedBatches.Select(b => b.Count));
            Assert.Equal(10, result.SuccessCount);
        }

        [Fact]
        public async Task ExecuteAsync_InvalidRow_IsReportedAndRemainingRowsStillImport()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream("Name,Score\nAlpha,50\n,60\nGamma,70");

            var result = await processor.ExecuteAsync(stream);

            Assert.Equal(3, result.TotalRowsRead);
            Assert.Equal(2, result.SuccessCount);
            Assert.Equal(1, result.FailureCount);
            Assert.Single(result.Errors);
            Assert.Equal(3, result.Errors[0].LineNumber);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task ExecuteAsync_StopOnFirstError_AbortsImmediately()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream("Name,Score\nAlpha,50\n,60\nGamma,70");

            var options = new ImportOptionsBuilder().StopOnFirstError().Build();
            var result = await processor.ExecuteAsync(stream, options);

            Assert.True(result.WasAborted);
            Assert.Equal(1, result.FailureCount);
            Assert.Equal(2, result.TotalRowsRead);      // Stopped before reading the third row.
        }

        [Fact]
        public async Task ExecuteAsync_ValidateOnly_ReportsResultsButPersistsNothing()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream("Name\nAlpha\nBeta");

            var options = new ImportOptionsBuilder().ValidateOnly().Build();
            var result = await processor.ExecuteAsync(stream, options);

            Assert.Equal(2, result.SuccessCount);
            Assert.True(result.ValidateOnly);
            Assert.Empty(processor.PersistedBatches);   // Nothing was written — a genuine dry run.
        }

        [Fact]
        public async Task ExecuteAsync_MapperReturningNull_CountsRowAsSkipped()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream("Name\nAlpha\nSKIP\nBeta");

            var result = await processor.ExecuteAsync(stream);

            Assert.Equal(1, result.SkippedCount);
            Assert.Equal(2, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
        }

        /// <summary>
        /// A single bad row must never take down a long-running import; the exception is captured as
        /// a row-level error and processing continues.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_MapperThrowing_RecordsErrorAndContinues()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream("Name\nAlpha\nTHROW\nBeta");

            var result = await processor.ExecuteAsync(stream);

            Assert.Equal(1, result.FailureCount);
            Assert.Equal(2, result.SuccessCount);
            Assert.Contains("Mapping deliberately failed", result.Errors[0].Message);
        }

        [Fact]
        public async Task ExecuteAsync_MissingRequiredColumn_ThrowsDescriptiveError()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream("WrongColumn\nAlpha");

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() => processor.ExecuteAsync(stream));

            Assert.Contains("Name", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_CapsTheNumberOfReportedErrors()
        {
            var processor = CreateProcessor();
            var csv = "Name,Score\n" + string.Join("\n", Enumerable.Repeat(",10", 50));
            await using var stream = TestData.CsvStream(csv);

            var options = new ImportOptionsBuilder().WithMaxReportedErrors(5).Build();
            var result = await processor.ExecuteAsync(stream, options);

            Assert.Equal(50, result.FailureCount);      // All failures are counted...
            Assert.Equal(5, result.Errors.Count);       // ...but only five details are retained.
        }

        [Fact]
        public async Task ExecuteAsync_RecordsDurationAndThroughput()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream(TestData.BuildStudentCsv(2_000)
                .Replace("StudentCode,FullName", "Name,FullName"));

            var result = await processor.ExecuteAsync(stream);

            Assert.True(result.Duration > TimeSpan.Zero);
            Assert.True(result.RowsPerSecond > 0);
            Assert.Contains("rows/s", result.Summary());
        }

        [Fact]
        public async Task ExecuteAsync_EmptyFileWithHeaderOnly_ReportsZeroRows()
        {
            var processor = CreateProcessor();
            await using var stream = TestData.CsvStream("Name,Score\n");

            var result = await processor.ExecuteAsync(stream);

            Assert.Equal(0, result.TotalRowsRead);
            Assert.True(result.IsSuccess);
            Assert.Empty(processor.PersistedBatches);
        }

        [Fact]
        public async Task ExecuteAsync_NullStream_Throws()
        {
            var processor = CreateProcessor();

            await Assert.ThrowsAsync<ArgumentNullException>(() => processor.ExecuteAsync(null!));
        }
    }
}
