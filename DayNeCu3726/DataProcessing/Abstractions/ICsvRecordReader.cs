namespace DayNeCu3726.DataProcessing.Abstractions
{
    /// <summary>
    /// Streams CSV records one row at a time.
    /// <para>
    /// Interface Segregation Principle (ISP): reading and writing are two separate contracts
    /// (<see cref="ICsvRecordReader"/> / <see cref="ICsvRecordWriter"/>) so an import-only class is
    /// never forced to depend on write operations it does not use.
    /// </para>
    /// <para>
    /// Returning <see cref="IAsyncEnumerable{T}"/> rather than a list is what makes the application a
    /// genuine large-dataset processor: memory usage stays constant regardless of file size, because
    /// only the current row is held in memory.
    /// </para>
    /// </summary>
    public interface ICsvRecordReader
    {
        /// <summary>Reads the header row, or an empty array when the source is empty.</summary>
        Task<IReadOnlyList<string>> ReadHeaderAsync(Stream source, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lazily streams every data row after the header. Each record maps column name to raw value.
        /// </summary>
        IAsyncEnumerable<CsvRecord> ReadRecordsAsync(Stream source, CancellationToken cancellationToken = default);
    }

    /// <summary>A single parsed CSV row together with its 1-based line number for error reporting.</summary>
    public sealed class CsvRecord
    {
        private readonly IReadOnlyDictionary<string, string> _fields;

        public int LineNumber { get; }
        public IReadOnlyList<string> RawValues { get; }

        public CsvRecord(int lineNumber, IReadOnlyList<string> rawValues, IReadOnlyDictionary<string, string> fields)
        {
            LineNumber = lineNumber;
            RawValues = rawValues;
            _fields = fields;
        }

        /// <summary>Returns the trimmed value of a column, or an empty string when the column is absent.</summary>
        public string this[string column] =>
            _fields.TryGetValue(column, out var value) ? value : string.Empty;

        public bool HasColumn(string column) => _fields.ContainsKey(column);
    }
}
