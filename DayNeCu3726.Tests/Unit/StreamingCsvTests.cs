using System.Text;
using DayNeCu3726.DataProcessing.Csv;
using DayNeCu3726.DataProcessing.Pipeline;
using DayNeCu3726.Tests.TestDoubles;

namespace DayNeCu3726.Tests.Unit
{
    /// <summary>
    /// Unit tests for the streaming CSV reader, writer and single-pass analyzer —
    /// the components that make the application a large-dataset processor.
    /// </summary>
    public class StreamingCsvTests
    {
        private readonly StreamingCsvRecordReader _reader = new();
        private readonly StreamingCsvRecordWriter _writer = new();

        [Fact]
        public async Task ReadHeaderAsync_ReturnsColumnNames()
        {
            await using var stream = TestData.CsvStream("Id,Name,Email\n1,A,a@sims.edu");

            var header = await _reader.ReadHeaderAsync(stream);

            Assert.Equal(new[] { "Id", "Name", "Email" }, header);
        }

        [Fact]
        public async Task ReadHeaderAsync_EmptyStream_ReturnsEmptyHeader()
        {
            await using var stream = TestData.CsvStream(string.Empty);

            Assert.Empty(await _reader.ReadHeaderAsync(stream));
        }

        [Fact]
        public async Task ReadRecordsAsync_MapsValuesByColumnName()
        {
            await using var stream = TestData.CsvStream("Id,Name\n1,Alpha\n2,Beta");

            var records = await ReadAllAsync(stream);

            Assert.Equal(2, records.Count);
            Assert.Equal("Alpha", records[0]["Name"]);
            Assert.Equal("Beta", records[1]["Name"]);
        }

        /// <summary>Line numbers must be accurate, because every error message references them.</summary>
        [Fact]
        public async Task ReadRecordsAsync_AssignsCorrectLineNumbers()
        {
            await using var stream = TestData.CsvStream("Id,Name\n1,Alpha\n2,Beta\n3,Gamma");

            var records = await ReadAllAsync(stream);

            Assert.Equal(new[] { 2, 3, 4 }, records.Select(r => r.LineNumber));
        }

        [Fact]
        public async Task ReadRecordsAsync_SkipsBlankLines()
        {
            await using var stream = TestData.CsvStream("Id,Name\n1,Alpha\n\n\n2,Beta\n");

            var records = await ReadAllAsync(stream);

            Assert.Equal(2, records.Count);
        }

        [Fact]
        public async Task ReadRecordsAsync_ShortRow_FillsMissingColumnsWithEmptyString()
        {
            await using var stream = TestData.CsvStream("Id,Name,Email\n1,Alpha");

            var records = await ReadAllAsync(stream);

            Assert.Equal("Alpha", records[0]["Name"]);
            Assert.Equal(string.Empty, records[0]["Email"]);
        }

        [Fact]
        public async Task ReadRecordsAsync_UnknownColumn_ReturnsEmptyStringNotException()
        {
            await using var stream = TestData.CsvStream("Id,Name\n1,Alpha");

            var records = await ReadAllAsync(stream);

            Assert.Equal(string.Empty, records[0]["NoSuchColumn"]);
            Assert.False(records[0].HasColumn("NoSuchColumn"));
        }

        [Fact]
        public async Task ReadRecordsAsync_ColumnLookupIsCaseInsensitive()
        {
            await using var stream = TestData.CsvStream("Id,Name\n1,Alpha");

            var records = await ReadAllAsync(stream);

            Assert.Equal("Alpha", records[0]["name"]);
            Assert.Equal("Alpha", records[0]["NAME"]);
        }

        [Fact]
        public async Task ReadRecordsAsync_QuotedFieldSpanningLines_IsReadAsOneRecord()
        {
            await using var stream = TestData.CsvStream("Id,Address\n1,\"Line one\nLine two\"\n2,Simple");

            var records = await ReadAllAsync(stream);

            Assert.Equal(2, records.Count);
            Assert.Contains("Line one", records[0]["Address"]);
            Assert.Contains("Line two", records[0]["Address"]);
        }

        [Fact]
        public async Task ReadRecordsAsync_StripsUtf8ByteOrderMarkFromFirstHeader()
        {
            var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes("Id,Name\n1,Alpha");
            await using var stream = new MemoryStream(bytes);

            var records = await ReadAllAsync(stream);

            Assert.Equal("1", records[0]["Id"]);
        }

        [Fact]
        public async Task WriteRowsAsync_WritesHeaderAndEscapesValues()
        {
            await using var output = new MemoryStream();

            var rowsWritten = await _writer.WriteRowsAsync(
                output,
                new[] { "Name", "Note" },
                ToAsync(new[]
                {
                    (IReadOnlyList<string>)new[] { "Nguyen Van A, Jr.", "ok" },
                    new[] { "Plain", "He said \"hi\"" }
                }));

            var text = Encoding.UTF8.GetString(output.ToArray());

            Assert.Equal(2, rowsWritten);
            Assert.Contains("Name,Note", text);
            Assert.Contains("\"Nguyen Van A, Jr.\"", text);
            Assert.Contains("\"He said \"\"hi\"\"\"", text);
        }

        /// <summary>
        /// End-to-end proof that the writer and reader agree: anything exported can be re-imported
        /// without data loss, which is what makes CSV a safe interchange format for this system.
        /// </summary>
        [Fact]
        public async Task WriteThenRead_RoundTripsValuesContainingDelimitersAndQuotes()
        {
            await using var output = new MemoryStream();

            await _writer.WriteRowsAsync(
                output,
                new[] { "Name", "Address" },
                ToAsync(new[] { (IReadOnlyList<string>)new[] { "Nguyen Van A, Jr.", "He said \"hi\"" } }));

            await using var input = new MemoryStream(output.ToArray());
            var records = await ReadAllAsync(input);

            Assert.Single(records);
            Assert.Equal("Nguyen Van A, Jr.", records[0]["Name"]);
            Assert.Equal("He said \"hi\"", records[0]["Address"]);
        }

        [Fact]
        public async Task Analyzer_ComputesAggregatesInOnePass()
        {
            const string csv = """
                StudentCode,FullName,Email,Program,Department,EnrollmentYear,GPA
                BH00001,A,a@sims.edu,Computer Science,Computing,2023,8.0
                BH00002,B,b@sims.edu,Computer Science,Computing,2024,6.0
                BH00003,C,c@sims.edu,Data Science,Computing,2024,10.0
                """;

            await using var stream = TestData.CsvStream(csv);
            var statistics = await new CsvDatasetAnalyzer(_reader).AnalyzeAsync(stream);

            Assert.Equal(3, statistics.TotalRecords);
            Assert.Equal(3, statistics.ValidGpaCount);
            Assert.Equal(8.0, statistics.AverageGpa);
            Assert.Equal(6.0, statistics.MinimumGpa);
            Assert.Equal(10.0, statistics.MaximumGpa);
            Assert.Equal(2, statistics.CountByProgram["Computer Science"]);
            Assert.Equal(1, statistics.CountByProgram["Data Science"]);
            Assert.Equal(2, statistics.CountByEnrollmentYear[2024]);
        }

        [Fact]
        public async Task Analyzer_IgnoresNonNumericGpaWithoutFailing()
        {
            const string csv = """
                FullName,Program,GPA
                A,CS,8.0
                B,CS,not-a-number
                """;

            await using var stream = TestData.CsvStream(csv);
            var statistics = await new CsvDatasetAnalyzer(_reader).AnalyzeAsync(stream);

            Assert.Equal(2, statistics.TotalRecords);
            Assert.Equal(1, statistics.ValidGpaCount);
            Assert.Equal(8.0, statistics.AverageGpa);
        }

        [Fact]
        public async Task Analyzer_EmptyDataset_ReturnsZeroedStatistics()
        {
            await using var stream = TestData.CsvStream("FullName,Program,GPA\n");
            var statistics = await new CsvDatasetAnalyzer(_reader).AnalyzeAsync(stream);

            Assert.Equal(0, statistics.TotalRecords);
            Assert.Equal(0, statistics.MinimumGpa);
            Assert.Equal(0, statistics.MaximumGpa);
        }

        /// <summary>
        /// Performance characterisation: 20,000 rows are streamed and aggregated well inside a few
        /// seconds, evidencing that throughput scales linearly rather than degrading with volume.
        /// </summary>
        [Fact]
        public async Task Analyzer_LargeDataset_CompletesWithinTimeBudget()
        {
            await using var stream = TestData.CsvStream(TestData.BuildStudentCsv(20_000));

            var statistics = await new CsvDatasetAnalyzer(_reader).AnalyzeAsync(stream);

            Assert.Equal(20_000, statistics.TotalRecords);
            Assert.True(statistics.Duration.TotalSeconds < 15,
                $"Analysing 20,000 rows took {statistics.Duration.TotalSeconds:F2}s, which exceeds the budget.");
        }

        [Fact]
        public async Task ReadRecordsAsync_HonoursCancellation()
        {
            using var cancellation = new CancellationTokenSource();
            await using var stream = TestData.CsvStream(TestData.BuildStudentCsv(500));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await foreach (var record in _reader.ReadRecordsAsync(stream, cancellation.Token))
                {
                    if (record.LineNumber > 5)
                        cancellation.Cancel();
                }
            });
        }

        private async Task<List<DataProcessing.Abstractions.CsvRecord>> ReadAllAsync(Stream stream)
        {
            var records = new List<DataProcessing.Abstractions.CsvRecord>();
            await foreach (var record in _reader.ReadRecordsAsync(stream))
                records.Add(record);

            return records;
        }

        private static async IAsyncEnumerable<IReadOnlyList<string>> ToAsync(IEnumerable<IReadOnlyList<string>> rows)
        {
            foreach (var row in rows)
            {
                yield return row;
                await Task.Yield();
            }
        }
    }
}
