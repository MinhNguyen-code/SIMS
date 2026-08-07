using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Patterns.Strategy
{
    /// <summary>
    /// Factory Method (creational) that selects the grading <see cref="IGradeStrategy"/> for a scheme.
    /// <para>
    /// The switch used to be copied inline into <c>EnrollmentService</c>'s constructor. Extracting it
    /// means adding a new grading scheme touches this one file instead of every service that grades —
    /// the Open/Closed Principle applied to strategy selection — and lets the mapping be unit tested
    /// on its own.
    /// </para>
    /// </summary>
    public static class GradeStrategyFactory
    {
        public static IGradeStrategy Create(GradingScheme scheme) => scheme switch
        {
            GradingScheme.Letter => new LetterGradeStrategy(),
            GradingScheme.Btec => new BtecGradeStrategy(),
            GradingScheme.Numeric => new NumericGradeStrategy(),
            _ => new NumericGradeStrategy()
        };
    }
}
