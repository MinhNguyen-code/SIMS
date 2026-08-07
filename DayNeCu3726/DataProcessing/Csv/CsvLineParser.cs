using System.Text;

namespace DayNeCu3726.DataProcessing.Csv
{
    /// <summary>
    /// RFC 4180 compliant field splitter.
    /// <para>
    /// The previous implementation used <c>line.Split(',')</c>, which silently corrupts any record
    /// containing a quoted comma — for example <c>"Nguyen Van A, Jr."</c> was split into two fields
    /// and shifted every subsequent column. This parser honours quoting and escaped quotes
    /// (<c>""</c>) so real-world data imports correctly.
    /// </para>
    /// <para>
    /// Single Responsibility Principle (SRP): this type does exactly one thing — turn one physical
    /// line into fields. Stream handling, mapping and validation live in their own classes.
    /// </para>
    /// </summary>
    public static class CsvLineParser
    {
        /// <summary>Splits a single CSV line into its fields, honouring quoted sections.</summary>
        public static IReadOnlyList<string> Split(string line, char delimiter = ',')
        {
            if (string.IsNullOrEmpty(line))
                return Array.Empty<string>();

            var fields = new List<string>();
            var current = new StringBuilder();
            var insideQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var character = line[i];

                if (insideQuotes)
                {
                    if (character == '"')
                    {
                        // A doubled quote inside a quoted field is an escaped literal quote.
                        var isEscapedQuote = i + 1 < line.Length && line[i + 1] == '"';
                        if (isEscapedQuote)
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            insideQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(character);
                    }
                }
                else if (character == '"')
                {
                    insideQuotes = true;
                }
                else if (character == delimiter)
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(character);
                }
            }

            fields.Add(current.ToString().Trim());
            return fields;
        }

        /// <summary>
        /// Escapes a value for CSV output, quoting it when it contains a delimiter, quote or newline.
        /// Prevents an exported file from becoming unparseable when re-imported.
        /// </summary>
        public static string Escape(string? value, char delimiter = ',')
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var needsQuoting = value.Contains(delimiter) ||
                               value.Contains('"') ||
                               value.Contains('\n') ||
                               value.Contains('\r');

            return needsQuoting
                ? '"' + value.Replace("\"", "\"\"") + '"'
                : value;
        }

        /// <summary>
        /// Determines whether a quoted field was left open, meaning the record continues on the next
        /// physical line (a multi-line CSV value).
        /// </summary>
        public static bool HasUnbalancedQuotes(string line)
        {
            var quoteCount = 0;
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] != '"') continue;

                if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    i++;      // Skip an escaped quote pair.
                    continue;
                }
                quoteCount++;
            }
            return quoteCount % 2 != 0;
        }
    }
}
