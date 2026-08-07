using System.Diagnostics;
using DayNeCu3726.DataProcessing.Abstractions;
using DayNeCu3726.DataProcessing.Validation;

namespace DayNeCu3726.DataProcessing.Pipeline
{
    /// <summary>
    /// Template Method pattern (behavioural) — defines the fixed skeleton of every bulk CSV import
    /// while letting subclasses supply the parts that differ per entity type.
    /// <para>
    /// The invariant algorithm is: stream rows → validate each row → map it to an entity → buffer it →
    /// commit whole batches → report. Subclasses cannot reorder or skip those steps; they only fill in
    /// <see cref="BuildValidationChain"/>, <see cref="MapRecord"/> and <see cref="PersistBatchAsync"/>.
    /// </para>
    /// <para>
    /// This removes the duplication that would otherwise appear once per import feature, and it is why
    /// students, courses and grades can all reuse the same tested batching and error-reporting logic.
    /// Open/Closed Principle: new import types extend the class instead of modifying it.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">Domain type produced from each CSV row.</typeparam>
    public abstract class BatchImportProcessor<TEntity> where TEntity : class
    {
        private readonly ICsvRecordReader _reader;

        protected BatchImportProcessor(ICsvRecordReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <summary>Columns the file must contain before any row is processed.</summary>
        protected abstract IReadOnlyList<string> RequiredColumns { get; }

        /// <summary>Builds the validation chain applied to every row. Called once per run.</summary>
        protected abstract RowValidationHandler BuildValidationChain();

        /// <summary>
        /// Converts a validated record into an entity. Returning <c>null</c> skips the row without
        /// counting it as an error (for example, a record that already exists and must not be updated).
        /// </summary>
        protected abstract TEntity? MapRecord(CsvRecord record, ImportOptions options);

        /// <summary>Persists one batch. Implementations decide between insert, update or upsert semantics.</summary>
        protected abstract Task PersistBatchAsync(IReadOnlyList<TEntity> batch, ImportOptions options, CancellationToken cancellationToken);

        /// <summary>Optional hook invoked after each committed batch, used here to report progress.</summary>
        protected virtual Task OnBatchCommittedAsync(int batchNumber, int rowsProcessed, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        /// <summary>
        /// The template method itself — <c>sealed</c> so the processing contract cannot be broken by a subclass.
        /// </summary>
        public async Task<ImportResult> ExecuteAsync(
            Stream csvSource,
            ImportOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(csvSource);

            options ??= ImportOptions.Default;
            var result = new ImportResult { ValidateOnly = options.ValidateOnly };
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await EnsureRequiredColumnsAsync(csvSource, cancellationToken);

                var validationChain = BuildValidationChain();
                var batch = new List<TEntity>(options.BatchSize);

                await foreach (var record in _reader.ReadRecordsAsync(csvSource, cancellationToken))
                {
                    result.RecordRowRead();

                    var validationError = validationChain.Handle(record);
                    if (validationError is not null)
                    {
                        result.RecordFailure(validationError, options.MaxReportedErrors);

                        if (options.StopOnFirstError)
                        {
                            result.MarkAborted();
                            break;
                        }
                        continue;
                    }

                    TEntity? entity;
                    try
                    {
                        entity = MapRecord(record, options);
                    }
                    catch (Exception ex)
                    {
                        // A mapping failure must never abort a 100 000-row import; report it and continue.
                        result.RecordFailure(new RowValidationError(record.LineNumber, "*", ex.Message), options.MaxReportedErrors);

                        if (!options.StopOnFirstError) continue;

                        result.MarkAborted();
                        break;
                    }

                    if (entity is null)
                    {
                        result.RecordSkipped();
                        continue;
                    }

                    batch.Add(entity);

                    if (batch.Count >= options.BatchSize)
                        await CommitBatchAsync(batch, options, result, cancellationToken);
                }

                if (batch.Count > 0)
                    await CommitBatchAsync(batch, options, result, cancellationToken);
            }
            finally
            {
                stopwatch.Stop();
                result.SetDuration(stopwatch);
            }

            return result;
        }

        /// <summary>
        /// Commits one buffered batch. Batching is what keeps the import fast: committing every row
        /// individually issues one database round-trip per record, whereas a batch of 1 000 issues one.
        /// </summary>
        private async Task CommitBatchAsync(
            List<TEntity> batch,
            ImportOptions options,
            ImportResult result,
            CancellationToken cancellationToken)
        {
            if (!options.ValidateOnly)
                await PersistBatchAsync(batch, options, cancellationToken);

            for (var i = 0; i < batch.Count; i++)
                result.RecordSuccess();

            result.RecordBatchCommitted();
            await OnBatchCommittedAsync(result.BatchesCommitted, result.TotalRowsRead, cancellationToken);

            batch.Clear();
        }

        private async Task EnsureRequiredColumnsAsync(Stream csvSource, CancellationToken cancellationToken)
        {
            var header = await _reader.ReadHeaderAsync(csvSource, cancellationToken);

            var missing = RequiredColumns
                .Where(required => !header.Contains(required, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missing.Count > 0)
                throw new InvalidDataException($"The CSV file is missing required column(s): {string.Join(", ", missing)}.");
        }
    }
}
