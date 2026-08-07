using DayNeCu3726.DataProcessing.Abstractions;

namespace DayNeCu3726.DataProcessing.Validation
{
    /// <summary>
    /// Chain of Responsibility (behavioural pattern) — base class for CSV row validation rules.
    /// <para>
    /// Each concrete handler checks exactly one rule and then passes the record along the chain.
    /// Adding a new rule means writing a new handler and linking it, never editing an existing one,
    /// which is the Open/Closed Principle applied to validation.
    /// </para>
    /// <para>
    /// The alternative — one long <c>if/else</c> block inside the importer — would have grown with
    /// every new rule and made the importer impossible to unit test in isolation.
    /// </para>
    /// </summary>
    public abstract class RowValidationHandler
    {
        private RowValidationHandler? _next;

        /// <summary>Links <paramref name="next"/> after this handler and returns it for fluent chaining.</summary>
        public RowValidationHandler SetNext(RowValidationHandler next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            return next;
        }

        /// <summary>
        /// Runs this rule and, when it passes, delegates to the rest of the chain.
        /// Returns the first failure encountered, or <c>null</c> when the record is valid.
        /// </summary>
        public RowValidationError? Handle(CsvRecord record)
        {
            var error = Validate(record);
            if (error is not null)
                return error;

            return _next?.Handle(record);
        }

        /// <summary>Implemented by each concrete rule. Returns <c>null</c> when the rule is satisfied.</summary>
        protected abstract RowValidationError? Validate(CsvRecord record);
    }

    /// <summary>Describes why a single CSV row was rejected, including its line number.</summary>
    public sealed record RowValidationError(int LineNumber, string Column, string Message)
    {
        public override string ToString() => $"Line {LineNumber} [{Column}]: {Message}";
    }
}
