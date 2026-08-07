namespace DayNeCu3726.Patterns.Strategy
{
    /// <summary>
    /// Concrete Strategy: letter grading scheme (A/B/C/D/F).
    /// <para>
    /// <b>Defect fixed:</b> this strategy previously evaluated grades on a 0–100 scale
    /// (<c>IsPassing(g) =&gt; g &gt;= 60</c>) while <see cref="BtecGradeStrategy"/> and
    /// <see cref="NumericGradeStrategy"/> both use the 0–10 scale that the rest of the system stores
    /// (see <c>Enrollment.Grade</c> and <c>SystemConfiguration.MinPassingGrade</c>). Because
    /// <c>EnrollmentService</c> selects a strategy at runtime, switching the configured scheme to
    /// <c>Letter</c> silently marked every passing student as failing — a genuine Liskov Substitution
    /// Principle violation, since the subtype did not honour the contract its callers relied on.
    /// </para>
    /// <para>
    /// The thresholds below are the 0–100 boundaries divided by ten, preserving the intended letter
    /// bands while making this strategy truly interchangeable with the others. An automated unit test
    /// (<c>PatternsTests.AllGradeStrategies_HonourTheSameContract</c>) now guards against a regression.
    /// </para>
    /// </summary>
    public class LetterGradeStrategy : IGradeStrategy
    {
        /// <summary>Lowest grade, on the shared 0–10 scale, that still counts as a pass.</summary>
        private const double PassingThreshold = 6.0;

        public string StrategyName => "Letter Grade (A-F)";

        public string CalculateLetterGrade(double numericGrade)
        {
            return numericGrade switch
            {
                >= 9.0 => "A",
                >= 8.0 => "B",
                >= 7.0 => "C",
                >= 6.0 => "D",
                _ => "F"
            };
        }

        public string GetGradeDescription(double numericGrade)
        {
            return numericGrade switch
            {
                >= 9.0 => "Excellent",
                >= 8.0 => "Good",
                >= 7.0 => "Average",
                >= 6.0 => "Below Average",
                _ => "Failing"
            };
        }

        public bool IsPassing(double numericGrade) => numericGrade >= PassingThreshold;

        public string GetGradePoint(double numericGrade)
        {
            return numericGrade switch
            {
                >= 9.0 => "4.0",
                >= 8.7 => "3.7",
                >= 8.3 => "3.3",
                >= 8.0 => "3.0",
                >= 7.7 => "2.7",
                >= 7.3 => "2.3",
                >= 7.0 => "2.0",
                >= 6.7 => "1.7",
                >= 6.3 => "1.3",
                >= 6.0 => "1.0",
                _ => "0.0"
            };
        }
    }
}
