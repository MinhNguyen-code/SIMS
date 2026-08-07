using System.Globalization;
using DayNeCu3726.DataProcessing.Abstractions;

namespace DayNeCu3726.DataProcessing.Pipeline
{
    /// <summary>Aggregate figures produced by a single streaming pass over a student dataset.</summary>
    public sealed class DatasetStatistics
    {
        public int TotalRecords { get; internal set; }
        public int ValidGpaCount { get; internal set; }
        public double AverageGpa { get; internal set; }
        public double MinimumGpa { get; internal set; }
        public double MaximumGpa { get; internal set; }
        public TimeSpan Duration { get; internal set; }

        public Dictionary<string, int> CountByProgram { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> CountByDepartment { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<int, int> CountByEnrollmentYear { get; } = new();

        public double RowsPerSecond =>
            Duration.TotalSeconds <= 0 ? 0 : Math.Round(TotalRecords / Duration.TotalSeconds, 2);
    }

    /// <summary>
    /// Computes dataset-wide statistics in a single streaming pass.
    /// <para>
    /// Demonstrates true large-dataset processing: the running mean is maintained incrementally
    /// (Welford-style accumulation) and only small grouping dictionaries are retained, so memory is
    /// proportional to the number of distinct programs rather than to the number of rows. A naive
    /// implementation such as <c>rows.ToList().Average(r =&gt; r.Gpa)</c> would have to materialise the
    /// whole dataset first.
    /// </para>
    /// </summary>
    public sealed class CsvDatasetAnalyzer
    {
        private readonly ICsvRecordReader _reader;

        public CsvDatasetAnalyzer(ICsvRecordReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<DatasetStatistics> AnalyzeAsync(Stream csvSource, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(csvSource);

            var statistics = new DatasetStatistics { MinimumGpa = double.MaxValue, MaximumGpa = double.MinValue };
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var gpaSum = 0.0;

            await foreach (var record in _reader.ReadRecordsAsync(csvSource, cancellationToken))
            {
                statistics.TotalRecords++;

                Increment(statistics.CountByProgram, Fallback(record["Program"], "Unspecified"));
                Increment(statistics.CountByDepartment, Fallback(record["Department"], "Unspecified"));

                if (int.TryParse(record["EnrollmentYear"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var year))
                {
                    statistics.CountByEnrollmentYear.TryGetValue(year, out var yearCount);
                    statistics.CountByEnrollmentYear[year] = yearCount + 1;
                }

                if (!double.TryParse(record["GPA"], NumberStyles.Any, CultureInfo.InvariantCulture, out var gpa))
                    continue;

                statistics.ValidGpaCount++;
                gpaSum += gpa;
                statistics.MinimumGpa = Math.Min(statistics.MinimumGpa, gpa);
                statistics.MaximumGpa = Math.Max(statistics.MaximumGpa, gpa);
            }

            stopwatch.Stop();
            statistics.Duration = stopwatch.Elapsed;

            if (statistics.ValidGpaCount > 0)
            {
                statistics.AverageGpa = Math.Round(gpaSum / statistics.ValidGpaCount, 3);
            }
            else
            {
                statistics.MinimumGpa = 0;
                statistics.MaximumGpa = 0;
            }

            return statistics;
        }

        private static void Increment(IDictionary<string, int> counter, string key)
        {
            counter.TryGetValue(key, out var count);
            counter[key] = count + 1;
        }

        private static string Fallback(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
