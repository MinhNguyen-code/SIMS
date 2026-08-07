namespace DayNeCu3726.DataProcessing.Pipeline
{
    /// <summary>
    /// Immutable configuration for a bulk import run.
    /// Built through <see cref="ImportOptionsBuilder"/> so callers cannot construct an invalid instance.
    /// </summary>
    public sealed class ImportOptions
    {
        /// <summary>Number of records accumulated before they are flushed to the database in one transaction.</summary>
        public int BatchSize { get; }

        /// <summary>When true the run aborts on the first invalid row; otherwise bad rows are skipped and reported.</summary>
        public bool StopOnFirstError { get; }

        /// <summary>Upper bound on collected error details, preventing an unbounded error list on a badly formed file.</summary>
        public int MaxReportedErrors { get; }

        /// <summary>When true rows are validated and counted but nothing is persisted — a safe "dry run" preview.</summary>
        public bool ValidateOnly { get; }

        /// <summary>Existing records matched by key are updated instead of being rejected as duplicates.</summary>
        public bool UpdateExisting { get; }

        internal ImportOptions(int batchSize, bool stopOnFirstError, int maxReportedErrors, bool validateOnly, bool updateExisting)
        {
            BatchSize = batchSize;
            StopOnFirstError = stopOnFirstError;
            MaxReportedErrors = maxReportedErrors;
            ValidateOnly = validateOnly;
            UpdateExisting = updateExisting;
        }

        public static ImportOptions Default => new ImportOptionsBuilder().Build();
    }

    /// <summary>
    /// Builder pattern (creational) — assembles <see cref="ImportOptions"/> step by step.
    /// <para>
    /// Without it, callers would face a five-argument constructor of mostly booleans
    /// (<c>new ImportOptions(1000, false, 100, false, true)</c>), which is unreadable and easy to get
    /// wrong. The fluent API names every choice at the call site and validates inputs centrally.
    /// </para>
    /// </summary>
    public sealed class ImportOptionsBuilder
    {
        private int _batchSize = 1_000;
        private bool _stopOnFirstError;
        private int _maxReportedErrors = 100;
        private bool _validateOnly;
        private bool _updateExisting;

        public ImportOptionsBuilder WithBatchSize(int batchSize)
        {
            if (batchSize < 1)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be at least 1.");

            _batchSize = batchSize;
            return this;
        }

        public ImportOptionsBuilder StopOnFirstError(bool stop = true)
        {
            _stopOnFirstError = stop;
            return this;
        }

        public ImportOptionsBuilder WithMaxReportedErrors(int maximum)
        {
            if (maximum < 0)
                throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum reported errors cannot be negative.");

            _maxReportedErrors = maximum;
            return this;
        }

        public ImportOptionsBuilder ValidateOnly(bool validateOnly = true)
        {
            _validateOnly = validateOnly;
            return this;
        }

        public ImportOptionsBuilder UpdateExisting(bool updateExisting = true)
        {
            _updateExisting = updateExisting;
            return this;
        }

        public ImportOptions Build() =>
            new(_batchSize, _stopOnFirstError, _maxReportedErrors, _validateOnly, _updateExisting);
    }
}
