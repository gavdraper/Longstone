using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.DayCount;
using Longstone.Domain.Instruments.Strategies;

namespace Longstone.Infrastructure.Instruments.Strategies;

public sealed class FixedIncomeValuationStrategy : IInstrumentValuationStrategy
{
    public decimal CalculateMarketValue(decimal quantity, decimal price)
    {
        return quantity * price;
    }

    public decimal CalculateAccruedIncome(Instrument instrument, DateTime settlementDate)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        var details = instrument.FixedIncomeDetails;
        if (details is null || details.CouponRate == 0m)
        {
            return 0m;
        }

        if (settlementDate < details.LastCouponDate)
        {
            throw new ArgumentOutOfRangeException(nameof(settlementDate), "Settlement date cannot be before the last coupon date.");
        }

        var calculator = DayCountCalculatorFactory.Create(details.DayCountConvention);
        var paymentsPerYear = (int)details.CouponFrequency;
        var couponPerPeriod = details.FaceValue * (details.CouponRate / paymentsPerYear);

        var accrualFraction = calculator.CalculateYearFraction(details.LastCouponDate, settlementDate);
        var periodFraction = calculator.CalculateYearFraction(details.LastCouponDate, details.NextCouponDate);

        if (periodFraction == 0m)
        {
            return 0m;
        }

        var accruedIncome = couponPerPeriod * (accrualFraction / periodFraction);

        return Math.Round(Math.Min(accruedIncome, couponPerPeriod), 6);
    }
}
