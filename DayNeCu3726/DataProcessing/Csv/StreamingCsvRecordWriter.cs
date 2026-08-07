using System.Text;
using DayNeCu3726.DataProcessing.Abstractions;

namespace DayNeCu3726.DataProcessing.Csv
{
    /// <summary>
    /// Writes CSV incrementally and flushes periodically, so exporting hundreds of thousands of rows
    /// streams straight to the HTTP response instead of being buffered in memory first.
    /// </summary>
    public sealed class StreamingCsvRecordWriter : ICsvRecordWriter
    {
        private const int FlushEveryRows = 1_000;
        private readonly char _delimiter;

        public StreamingCsvRecordWriter(char delimiter = ',')
        {
            _delimiter = delimiter;
        }

        public async Task WriteHeaderAsync(Stream destination, IReadOnlyList<string> columns, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(destination);
            ArgumentNullException.ThrowIfNull(columns);

            await using var writer = CreateWriter(destination);
            await writer.WriteLineAsync(FormatRow(columns));
            await writer.FlushAsync(cancellationToken);
        }

        public async Task<int> WriteRowsAsync(
            Stream destination,
            IReadOnlyList<string> columns,
            IAsyncEnumerable<IReadOnlyList<string>> rows,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(destination);
            ArgumentNullException.ThrowIfNull(columns);
            ArgumentNullException.ThrowIfNull(rows);

            await using var writer = CreateWriter(destination);
            await writer.WriteLineAsync(FormatRow(columns));

            var rowsWritten = 0;
            await foreach (var row in rows.WithCancellation(cancellationToken))
            {
                await writer.WriteLineAsync(FormatRow(row));
                rowsWritten++;

                if (rowsWritten % FlushEveryRows == 0)
                    await writer.FlushAsync(cancellationToken);
            }

            await writer.FlushAsync(cancellationToken);
            return rowsWritten;
        }

        private string FormatRow(IReadOnlyList<string> values) =>
            string.Join(_delimiter, values.Select(v => CsvLineParser.Escape(v, _delimiter)));

        private static StreamWriter CreateWriter(Stream destination) =>
            new(destination, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 64 * 1024, leaveOpen: true);
    }
}
