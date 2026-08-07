using System.Diagnostics;
using DayNeCu3726.DataProcessing.Validation;

namespace DayNeCu3726.DataProcessing.Pipeline
{
    /// <summary>
    /// Outcome of a bulk import run: counts, timing and a bounded list of row-level errors.
    /// <para>
    /// The previous grade import reported only "Successfully updated N student records" and silently
    /// discarded every malformed row, so an administrator had no way to discover which rows failed or
    /// why. This result object makes failures explicit and auditable.
    /// </para>
    /// </summary>
    public sealed class ImportResult
    {
        private readonly List<RowValidationError> _errors = new();

        public int TotalRowsRead { get; private set; }
        public int SuccessCount { get; private set; }
        public int FailureCount { get; private set; }
        public int SkippedCount { get; private set; }
        public int BatchesCommitted { get; private set; }
        public TimeSpan Duration { get; private set; }
        public bool WasAborted { get; private set; }
        public bool ValidateOnly { get; init; }

        public IReadOnlyList<RowValidationError> Errors => _errors;
        public bool IsSuccess => FailureCount == 0 && !WasAborted;

        /// <summary>Throughput in rows per second — useful evidence when demonstrating scalability.</summary>
        public double RowsPerSecond =>
            Duration.TotalSeconds <= 0 ? 0 : Math.Round(TotalRowsRead / Duration.TotalSeconds, 2);

        internal void RecordRowRead() => TotalRowsRead++;
        internal void RecordSuccess() => SuccessCount++;
        internal void RecordSkipped() => SkippedCount++;
        internal void RecordBatchCommitted() => BatchesCommitted++;
        internal void MarkAborted() => WasAborted = true;
        internal void SetDuration(Stopwatch stopwatch) => Duration = stopwatch.Elapsed;

        internal void RecordFailure(RowValidationError error, int maxReportedErrors)
        {
            FailureCount++;

            // Cap the stored details so a file where every row is broken cannot exhaust memory.
            if (_errors.Count < maxReportedErrors)
                _errors.Add(error);
        }

        public string Summary()
        {
            var mode = ValidateOnly ? "Validation" : "Import";
            return $"{mode} finished: {SuccessCount} succeeded, {FailureCount} failed, {SkippedCount} skipped " +
                   $"out of {TotalRowsRead} rows in {Duration.TotalSeconds:F2}s ({RowsPerSecond} rows/s).";
        }
    }
}
