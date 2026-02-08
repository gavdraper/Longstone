namespace Longstone.Domain.Instruments.DayCount;

public interface IDayCountCalculator
{
    decimal CalculateYearFraction(DateTime startDate, DateTime endDate);
}
