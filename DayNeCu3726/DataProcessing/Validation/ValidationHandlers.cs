using System.Globalization;
using System.Text.RegularExpressions;
using DayNeCu3726.DataProcessing.Abstractions;

namespace DayNeCu3726.DataProcessing.Validation
{
    /// <summary>Rejects rows where any mandatory column is blank.</summary>
    public sealed class RequiredColumnsHandler : RowValidationHandler
    {
        private readonly IReadOnlyList<string> _requiredColumns;

        public RequiredColumnsHandler(params string[] requiredColumns)
        {
            _requiredColumns = requiredColumns ?? Array.Empty<string>();
        }

        protected override RowValidationError? Validate(CsvRecord record)
        {
            foreach (var column in _requiredColumns)
            {
                if (string.IsNullOrWhiteSpace(record[column]))
                    return new RowValidationError(record.LineNumber, column, "Value is required but was empty.");
            }
            return null;
        }
    }

    /// <summary>Rejects rows whose email column is not a syntactically valid address.</summary>
    public sealed partial class EmailFormatHandler : RowValidationHandler
    {
        private readonly string _column;

        public EmailFormatHandler(string column = "Email")
        {
            _column = column;
        }

        protected override RowValidationError? Validate(CsvRecord record)
        {
            var value = record[_column];
            if (string.IsNullOrWhiteSpace(value))
                return null;   // Presence is the RequiredColumnsHandler's responsibility, not ours (SRP).

            return EmailPattern().IsMatch(value)
                ? null
                : new RowValidationError(record.LineNumber, _column, $"'{value}' is not a valid email address.");
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
        private static partial Regex EmailPattern();
    }

    /// <summary>Rejects rows whose numeric column falls outside an allowed inclusive range.</summary>
    public sealed class NumericRangeHandler : RowValidationHandler
    {
        private readonly string _column;
        private readonly double _minimum;
        private readonly double _maximum;
        private readonly bool _optional;

        public NumericRangeHandler(string column, double minimum, double maximum, bool optional = false)
        {
            _column = column;
            _minimum = minimum;
            _maximum = maximum;
            _optional = optional;
        }

        protected override RowValidationError? Validate(CsvRecord record)
        {
            var value = record[_column];

            if (string.IsNullOrWhiteSpace(value))
                return _optional ? null : new RowValidationError(record.LineNumber, _column, "A numeric value is required.");

            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                return new RowValidationError(record.LineNumber, _column, $"'{value}' is not a number.");

            return parsed < _minimum || parsed > _maximum
                ? new RowValidationError(record.LineNumber, _column, $"Value {parsed} is outside the allowed range {_minimum}–{_maximum}.")
                : null;
        }
    }

    /// <summary>
    /// Rejects a row whose key duplicates one already seen earlier in the same file.
    /// Keeping the seen-keys set inside the handler means the importer stays free of bookkeeping state.
    /// </summary>
    public sealed class UniqueValueHandler : RowValidationHandler
    {
        private readonly string _column;
        private readonly HashSet<string> _seenValues = new(StringComparer.OrdinalIgnoreCase);

        public UniqueValueHandler(string column)
        {
            _column = column;
        }

        protected override RowValidationError? Validate(CsvRecord record)
        {
            var value = record[_column];
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return _seenValues.Add(value)
                ? null
                : new RowValidationError(record.LineNumber, _column, $"'{value}' is duplicated within the file.");
        }
    }

    /// <summary>Rejects rows whose date column cannot be parsed by any accepted format.</summary>
    public sealed class DateFormatHandler : RowValidationHandler
    {
        private static readonly string[] AcceptedFormats =
            { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd" };

        private readonly string _column;
        private readonly bool _optional;

        public DateFormatHandler(string column, bool optional = true)
        {
            _column = column;
            _optional = optional;
        }

        protected override RowValidationError? Validate(CsvRecord record)
        {
            var value = record[_column];

            if (string.IsNullOrWhiteSpace(value))
                return _optional ? null : new RowValidationError(record.LineNumber, _column, "A date is required.");

            return DateTime.TryParseExact(value, AcceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                ? null
                : new RowValidationError(record.LineNumber, _column, $"'{value}' is not a recognised date (expected yyyy-MM-dd).");
        }
    }
}
