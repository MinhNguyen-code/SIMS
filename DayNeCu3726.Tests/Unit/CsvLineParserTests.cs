using DayNeCu3726.DataProcessing.Csv;

namespace DayNeCu3726.Tests.Unit
{
    /// <summary>
    /// Unit tests for the RFC 4180 field parser.
    /// <para>
    /// These are pure unit tests: no database, no file system, no HTTP. Each one exercises a single
    /// function in isolation and runs in microseconds, which is exactly the property that lets the
    /// whole suite run on every commit.
    /// </para>
    /// </summary>
    public class CsvLineParserTests
    {
        [Fact]
        public void Split_SimpleLine_ReturnsEachField()
        {
            // Arrange (Chuẩn bị dữ liệu CSV)
            var csvLine = "BH00001,Nguyen Van A,a@sims.edu";

            // Act (Thực thi hàm Split để tách dòng CSV)
            var fields = CsvLineParser.Split(csvLine);

            // Assert (Kiểm chứng kết quả tách có đúng số lượng và nội dung không)
            Assert.Equal(3, fields.Count);
            Assert.Equal("BH00001", fields[0]);
            Assert.Equal("Nguyen Van A", fields[1]);
            Assert.Equal("a@sims.edu", fields[2]);
        }

        /// <summary>
        /// Regression test for the defect in the original importer: <c>string.Split(',')</c> broke a
        /// quoted field containing a comma into two fields and shifted every later column.
        /// </summary>
        [Fact]
        public void Split_QuotedFieldContainingComma_KeepsFieldIntact()
        {
            var fields = CsvLineParser.Split("BH00001,\"Nguyen Van A, Jr.\",a@sims.edu");

            Assert.Equal(3, fields.Count);
            Assert.Equal("Nguyen Van A, Jr.", fields[1]);
            Assert.Equal("a@sims.edu", fields[2]);
        }

        [Fact]
        public void Split_EscapedDoubleQuote_UnescapesToSingleQuote()
        {
            var fields = CsvLineParser.Split("\"He said \"\"hello\"\"\",second");

            Assert.Equal("He said \"hello\"", fields[0]);
            Assert.Equal("second", fields[1]);
        }

        [Fact]
        public void Split_EmptyFields_ArePreserved()
        {
            var fields = CsvLineParser.Split("a,,c");

            Assert.Equal(3, fields.Count);
            Assert.Equal(string.Empty, fields[1]);
        }

        [Fact]
        public void Split_EmptyLine_ReturnsNoFields()
        {
            Assert.Empty(CsvLineParser.Split(string.Empty));
        }

        [Theory]
        [InlineData("plain", "plain")]
        [InlineData("has,comma", "\"has,comma\"")]
        [InlineData("has\"quote", "\"has\"\"quote\"")]
        [InlineData("has\nnewline", "\"has\nnewline\"")]
        public void Escape_QuotesOnlyWhenNecessary(string input, string expected)
        {
            Assert.Equal(expected, CsvLineParser.Escape(input));
        }

        /// <summary>Round-tripping proves an exported file can be re-imported without corruption.</summary>
        [Theory]
        [InlineData("Nguyen Van A, Jr.")]
        [InlineData("He said \"hello\"")]
        [InlineData("plain value")]
        public void EscapeThenSplit_ReturnsOriginalValue(string original)
        {
            var escaped = CsvLineParser.Escape(original);
            var parsed = CsvLineParser.Split(escaped);

            Assert.Single(parsed);
            Assert.Equal(original, parsed[0]);
        }

        [Theory]
        [InlineData("a,b,c", false)]
        [InlineData("a,\"b,c", true)]
        [InlineData("a,\"b\",c", false)]
        [InlineData("a,\"He said \"\"hi\"\"\",c", false)]
        public void HasUnbalancedQuotes_DetectsMultiLineRecords(string line, bool expected)
        {
            Assert.Equal(expected, CsvLineParser.HasUnbalancedQuotes(line));
        }

        [Fact]
        public void Split_CustomDelimiter_IsHonoured()
        {
            var fields = CsvLineParser.Split("a;b;c", ';');

            Assert.Equal(new[] { "a", "b", "c" }, fields);
        }
    }
}
