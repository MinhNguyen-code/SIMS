namespace DayNeCu3726.Patterns.Strategy
{
    /// <summary>
    /// Strategy Pattern – defines the interface for grade calculation algorithms.
    /// Allows switching between different grading schemes at runtime.
    /// </summary>
    public interface IGradeStrategy
    {
        string StrategyName { get; }
        string CalculateLetterGrade(double numericGrade);
        string GetGradeDescription(double numericGrade);
        bool IsPassing(double numericGrade);
        string GetGradePoint(double numericGrade);
    }
}
