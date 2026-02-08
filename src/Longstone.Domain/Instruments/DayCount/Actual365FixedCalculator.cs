namespace Longstone.Domain.Instruments.DayCount;

public sealed class Actual365FixedCalculator : IDayCountCalculator
{
    public decimal CalculateYearFraction(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be on or after start date.", nameof(endDate));
        }

        var days = (endDate - startDate).Days;
        return days / 365m;
    }
}
