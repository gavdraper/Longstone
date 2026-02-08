namespace Longstone.Domain.Instruments.DayCount;

public static class DayCountCalculatorFactory
{
    public static IDayCountCalculator Create(DayCountConvention convention)
    {
        return convention switch
        {
            DayCountConvention.ActualActualIsda => new ActualActualIsdaCalculator(),
            DayCountConvention.Actual365Fixed => new Actual365FixedCalculator(),
            DayCountConvention.Thirty360 => new Thirty360Calculator(),
            _ => throw new ArgumentOutOfRangeException(nameof(convention), convention, "Unsupported day count convention.")
        };
    }
}
