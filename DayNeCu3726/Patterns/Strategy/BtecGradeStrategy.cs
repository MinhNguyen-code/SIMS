namespace DayNeCu3726.Patterns.Strategy
{
    /// <summary>
    /// Concrete Strategy: BTEC grading scheme (Pass/Merit/Distinction/Fail).
    /// </summary>
    public class BtecGradeStrategy : IGradeStrategy
    {
        public string StrategyName => "BTEC (P/M/D)";

        public string CalculateLetterGrade(double numericGrade)
        {
            return numericGrade switch
            {
                >= 8.5 => "D",  // Distinction
                >= 7.0 => "M",  // Merit
                >= 5.0 => "P",  // Pass
                _ => "F"        // Fail
            };
        }

        public string GetGradeDescription(double numericGrade)
        {
            return numericGrade switch
            {
                >= 8.5 => "Distinction",
                >= 7.0 => "Merit",
                >= 5.0 => "Pass",
                _ => "Fail"
            };
        }

        public bool IsPassing(double numericGrade) => numericGrade >= 5.0;

        public string GetGradePoint(double numericGrade)
        {
            // BTEC doesn't use standard GPA. We return raw numeric equivalent for internal sorting if needed.
            return numericGrade switch
            {
                >= 8.5 => "4.0",
                >= 7.0 => "3.0",
                >= 5.0 => "2.0",
                _ => "0.0"
            };
        }
    }
}
