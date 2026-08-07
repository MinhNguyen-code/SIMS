namespace DayNeCu3726.Patterns.Strategy
{
    /// <summary>
    /// Concrete Strategy: Numeric grading scheme (0–10 scale, Vietnamese standard).
    /// </summary>
    public class NumericGradeStrategy : IGradeStrategy
    {
        public string StrategyName => "Numeric (0-10)";

        public string CalculateLetterGrade(double numericGrade)
        {
            return numericGrade switch
            {
                >= 9.0 => "A+",
                >= 8.5 => "A",
                >= 8.0 => "B+",
                >= 7.0 => "B",
                >= 6.5 => "C+",
                >= 5.5 => "C",
                >= 5.0 => "D+",
                >= 4.0 => "D",
                _ => "F"
            };
        }

        public string GetGradeDescription(double numericGrade)
        {
            return numericGrade switch
            {
                >= 9.0 => "Excellent",
                >= 8.0 => "Good",
                >= 7.0 => "Fair",
                >= 5.0 => "Average",
                >= 4.0 => "Weak",
                _ => "Poor"
            };
        }

        public bool IsPassing(double numericGrade) => numericGrade >= 5.0;

        public string GetGradePoint(double numericGrade)
        {
            return numericGrade switch
            {
                >= 9.0 => "4.0",
                >= 8.5 => "4.0",
                >= 8.0 => "3.5",
                >= 7.0 => "3.0",
                >= 6.5 => "2.5",
                >= 5.5 => "2.0",
                >= 5.0 => "1.5",
                >= 4.0 => "1.0",
                _ => "0.0"
            };
        }
    }
}
