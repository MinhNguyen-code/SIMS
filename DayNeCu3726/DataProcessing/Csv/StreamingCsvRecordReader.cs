using System.Runtime.CompilerServices;
using System.Text;
using DayNeCu3726.DataProcessing.Abstractions;

namespace DayNeCu3726.DataProcessing.Csv
{
    /// <summary>
    /// Reads a CSV source row by row with constant memory usage.
    /// <para>
    /// This is the component that makes SIMS a large-dataset processing application. The previous
    /// grade import called <c>reader.ReadToEndAsync()</c>, allocating the entire file as one string
    /// plus an array of every line — a 500 MB CSV needed well over a gigabyte of RAM and frequently
    /// crashed the request. Here the underlying <see cref="StreamReader"/> keeps only a small buffer,
    /// so a file of any size is processed in a bounded working set.
    /// </para>
    /// <para>Liskov Substitution Principle (LSP): any <see cref="ICsvRecordReader"/> may replace this
    /// class — tests substitute an in-memory reader without changing the calling pipeline.</para>
    /// </summary>
    public sealed class StreamingCsvRecordReader : ICsvRecordReader
    {
        private readonly char _delimiter;

        public StreamingCsvRecordReader(char delimiter = ',')
        {
            _delimiter = delimiter;
        }

        public async Task<IReadOnlyList<string>> ReadHeaderAsync(Stream source, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            using var reader = CreateReader(source, leaveOpen: true);
            var headerLine = await reader.ReadLineAsync(cancellationToken);

            return headerLine is null
                ? Array.Empty<string>()
                : NormaliseHeader(CsvLineParser.Split(headerLine, _delimiter));
        }

        public async IAsyncEnumerable<CsvRecord> ReadRecordsAsync(
            Stream source,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            using var reader = CreateReader(source, leaveOpen: true);

            var headerLine = await reader.ReadLineAsync(cancellationToken);
            if (headerLine is null)
                yield break;

            var header = NormaliseHeader(CsvLineParser.Split(headerLine, _delimiter));
            var lineNumber = 1;

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null)
                    break;

                lineNumber++;

                // Join continuation lines belonging to a quoted field that spans multiple lines.
                while (CsvLineParser.HasUnbalancedQuotes(line) && !reader.EndOfStream)
                {
                    var continuation = await reader.ReadLineAsync(cancellationToken);
                    if (continuation is null) break;

                    line += "\n" + continuation;
                    lineNumber++;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;   // Blank separator lines are not data and must not fail the import.

                var values = CsvLineParser.Split(line, _delimiter);
                yield return new CsvRecord(lineNumber, values, BuildFieldMap(header, values));
            }
        }

        /// <summary>Pairs header names with row values, tolerating rows that are short or over-long.</summary>
        private static Dictionary<string, string> BuildFieldMap(IReadOnlyList<string> header, IReadOnlyList<string> values)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < header.Count; i++)
                map[header[i]] = i < values.Count ? values[i] : string.Empty;

            return map;
        }

        /// <summary>Strips the UTF-8 byte order mark and trims header names so lookups are reliable.</summary>
        private static IReadOnlyList<string> NormaliseHeader(IReadOnlyList<string> header)
        {
            if (header.Count == 0)
                return header;

            var cleaned = header.Select(h => h.Trim()).ToList();
            cleaned[0] = cleaned[0].TrimStart('\uFEFF');
            return cleaned;
        }

        private static StreamReader CreateReader(Stream source, bool leaveOpen)
        {
            if (source.CanSeek)
                source.Position = 0;

            return new StreamReader(source, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: leaveOpen);
        }
    }
}
