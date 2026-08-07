namespace DayNeCu3726.DataProcessing.Abstractions
{
    /// <summary>
    /// Writes CSV output incrementally so that exporting a very large result set never requires
    /// building the whole document in memory or on disk first.
    /// </summary>
    public interface ICsvRecordWriter
    {
        Task WriteHeaderAsync(Stream destination, IReadOnlyList<string> columns, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams <paramref name="rows"/> to <paramref name="destination"/> and returns the number of rows written.
        /// </summary>
        Task<int> WriteRowsAsync(
            Stream destination,
            IReadOnlyList<string> columns,
            IAsyncEnumerable<IReadOnlyList<string>> rows,
            CancellationToken cancellationToken = default);
    }
}
