namespace Longstone.Domain.Instruments.DayCount;

public sealed class ActualActualIsdaCalculator : IDayCountCalculator
{
    public decimal CalculateYearFraction(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be on or after start date.", nameof(endDate));
        }

        if (startDate == endDate)
        {
            return 0m;
        }

        var fraction = 0m;
        var current = startDate;

        while (current.Year < endDate.Year)
        {
            var yearEnd = new DateTime(current.Year + 1, 1, 1);
            var daysInYear = DateTime.IsLeapYear(current.Year) ? 366m : 365m;
            var daysInPeriod = (yearEnd - current).Days;

            fraction += daysInPeriod / daysInYear;
            current = yearEnd;
        }

        if (current < endDate)
        {
            var daysInYear = DateTime.IsLeapYear(current.Year) ? 366m : 365m;
            var daysInPeriod = (endDate - current).Days;

            fraction += daysInPeriod / daysInYear;
        }

        return fraction;
    }
}
