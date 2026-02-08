namespace Longstone.Domain.Instruments.DayCount;

public sealed class Thirty360Calculator : IDayCountCalculator
{
    public decimal CalculateYearFraction(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be on or after start date.", nameof(endDate));
        }

        var d1 = startDate.Day;
        var d2 = endDate.Day;

        if (d1 == 31)
        {
            d1 = 30;
        }

        if (d2 == 31 && d1 == 30)
        {
            d2 = 30;
        }

        var days = ((endDate.Year - startDate.Year) * 360)
                 + ((endDate.Month - startDate.Month) * 30)
                 + (d2 - d1);

        return days / 360m;
    }
}
