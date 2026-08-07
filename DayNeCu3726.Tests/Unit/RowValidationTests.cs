using DayNeCu3726.DataProcessing.Abstractions;
using DayNeCu3726.DataProcessing.Validation;

namespace DayNeCu3726.Tests.Unit
{
    /// <summary>
    /// Unit tests for the Chain of Responsibility validation handlers.
    /// Each handler is tested alone, then the assembled chain is tested as a whole.
    /// </summary>
    public class RowValidationTests
    {
        private static CsvRecord Record(int lineNumber = 2, params (string Column, string Value)[] fields)
        {
            var map = fields.ToDictionary(f => f.Column, f => f.Value, StringComparer.OrdinalIgnoreCase);
            return new CsvRecord(lineNumber, map.Values.ToList(), map);
        }

        [Fact]
        public void RequiredColumnsHandler_AllPresent_ReturnsNoError()
        {
            var handler = new RequiredColumnsHandler("FullName", "Email");
            var record = Record(2, ("FullName", "Nguyen Van A"), ("Email", "a@sims.edu"));

            Assert.Null(handler.Handle(record));
        }

        [Fact]
        public void RequiredColumnsHandler_MissingValue_ReportsColumnAndLine()
        {
            var handler = new RequiredColumnsHandler("FullName", "Email");
            var record = Record(7, ("FullName", "   "), ("Email", "a@sims.edu"));

            var error = handler.Handle(record);

            Assert.NotNull(error);
            Assert.Equal("FullName", error!.Column);
            Assert.Equal(7, error.LineNumber);
        }

        [Theory]
        [InlineData("a@sims.edu", true)]
        [InlineData("first.last@sub.domain.edu.vn", true)]
        [InlineData("no-at-sign", false)]
        [InlineData("missing@domain", false)]
        [InlineData("spaces in@sims.edu", false)]
        public void EmailFormatHandler_ValidatesSyntax(string email, bool expectedValid)
        {
            var handler = new EmailFormatHandler();
            var error = handler.Handle(Record(2, ("Email", email)));

            Assert.Equal(expectedValid, error is null);
        }

        /// <summary>
        /// Confirms Single Responsibility: the email handler ignores an empty value because
        /// enforcing presence belongs to <see cref="RequiredColumnsHandler"/>.
        /// </summary>
        [Fact]
        public void EmailFormatHandler_EmptyValue_DefersToRequiredHandler()
        {
            var handler = new EmailFormatHandler();

            Assert.Null(handler.Handle(Record(2, ("Email", ""))));
        }

        [Theory]
        [InlineData("7.5", true)]
        [InlineData("0", true)]
        [InlineData("10", true)]
        [InlineData("10.1", false)]
        [InlineData("-1", false)]
        [InlineData("abc", false)]
        public void NumericRangeHandler_EnforcesInclusiveBounds(string gpa, bool expectedValid)
        {
            var handler = new NumericRangeHandler("GPA", 0, 10, optional: true);
            var error = handler.Handle(Record(2, ("GPA", gpa)));

            Assert.Equal(expectedValid, error is null);
        }

        [Fact]
        public void NumericRangeHandler_WhenNotOptional_EmptyValueFails()
        {
            var handler = new NumericRangeHandler("GPA", 0, 10, optional: false);

            Assert.NotNull(handler.Handle(Record(2, ("GPA", ""))));
        }

        [Fact]
        public void UniqueValueHandler_SecondOccurrence_IsRejected()
        {
            var handler = new UniqueValueHandler("Email");

            Assert.Null(handler.Handle(Record(2, ("Email", "dup@sims.edu"))));

            var error = handler.Handle(Record(3, ("Email", "DUP@sims.edu")));   // Case-insensitive.

            Assert.NotNull(error);
            Assert.Equal(3, error!.LineNumber);
            Assert.Contains("duplicated", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("2004-05-20", true)]
        [InlineData("20/05/2004", true)]
        [InlineData("2004/05/20", true)]
        [InlineData("20-May-2004", false)]
        [InlineData("not-a-date", false)]
        public void DateFormatHandler_AcceptsKnownFormatsOnly(string date, bool expectedValid)
        {
            var handler = new DateFormatHandler("DateOfBirth");

            Assert.Equal(expectedValid, handler.Handle(Record(2, ("DateOfBirth", date))) is null);
        }

        /// <summary>The chain must stop at the first failing rule and report that rule's error.</summary>
        [Fact]
        public void Chain_ReportsFirstFailureAndStops()
        {
            var head = new RequiredColumnsHandler("FullName", "Email");
            head.SetNext(new EmailFormatHandler("Email"))
                .SetNext(new NumericRangeHandler("GPA", 0, 10, optional: true));

            var record = Record(4, ("FullName", "Nguyen Van A"), ("Email", "broken"), ("GPA", "99"));

            var error = head.Handle(record);

            Assert.NotNull(error);
            Assert.Equal("Email", error!.Column);   // Email fails before GPA is ever examined.
        }

        [Fact]
        public void Chain_AllRulesPass_ReturnsNull()
        {
            var head = new RequiredColumnsHandler("FullName", "Email");
            head.SetNext(new EmailFormatHandler("Email"))
                .SetNext(new NumericRangeHandler("GPA", 0, 10, optional: true));

            var record = Record(4, ("FullName", "Nguyen Van A"), ("Email", "a@sims.edu"), ("GPA", "8.25"));

            Assert.Null(head.Handle(record));
        }

        [Fact]
        public void RowValidationError_ToString_IncludesLineColumnAndMessage()
        {
            var error = new RowValidationError(12, "Email", "Invalid address.");

            Assert.Equal("Line 12 [Email]: Invalid address.", error.ToString());
        }
    }
}
