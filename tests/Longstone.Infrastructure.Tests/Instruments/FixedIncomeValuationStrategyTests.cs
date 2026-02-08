using FluentAssertions;
using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.DayCount;
using Longstone.Infrastructure.Instruments.Strategies;
using Microsoft.Extensions.Time.Testing;

namespace Longstone.Infrastructure.Tests.Instruments;

public class FixedIncomeValuationStrategyTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly FixedIncomeValuationStrategy _strategy = new();

    private Instrument CreateFixedIncomeInstrument(FixedIncomeDetails? details = null)
    {
        var instrument = Instrument.Create(
            "GB00B24CGK77", "B24CGK7", "T4Q",
            Exchange.LSE, "UK Gilt 4.25% 2036", "GBP", "GB", "Government",
            AssetClass.FixedIncome, 0m, _timeProvider);

        if (details is not null)
        {
            instrument.SetFixedIncomeDetails(details, _timeProvider);
        }

        return instrument;
    }

    // Market value tests

    [Fact]
    public void CalculateMarketValue_ReturnsQuantityTimesPrice()
    {
        var result = _strategy.CalculateMarketValue(1000m, 98.50m);

        result.Should().Be(98_500m);
    }

    [Fact]
    public void CalculateMarketValue_WithZeroQuantity_ReturnsZero()
    {
        var result = _strategy.CalculateMarketValue(0m, 98.50m);

        result.Should().Be(0m);
    }

    // Accrued income: UK gilt (semi-annual, ACT/ACT ISDA)

    [Fact]
    public void CalculateAccruedIncome_UkGilt_SemiAnnual_ActActIsda_ReturnsExpected()
    {
        // UK Gilt: 5% coupon, semi-annual, ACT/ACT ISDA, face value 100
        // Last coupon: 15 Jan 2025, next coupon: 15 Jul 2025
        // Settlement: 15 Apr 2025
        // Coupon per period = 100 * (0.05 / 2) = 2.50
        // Accrual days (15 Jan to 15 Apr) = 90 days in 2025 (non-leap, 365)
        // Period days (15 Jan to 15 Jul) = 181 days in 2025 (non-leap, 365)
        // accrualFraction = 90/365, periodFraction = 181/365
        // accruedIncome = 2.50 * (90/365) / (181/365) = 2.50 * 90/181
        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2036, 1, 15),
            couponFrequency: CouponFrequency.SemiAnnual,
            dayCountConvention: DayCountConvention.ActualActualIsda,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        var instrument = CreateFixedIncomeInstrument(details);
        var settlementDate = new DateTime(2025, 4, 15);

        var result = _strategy.CalculateAccruedIncome(instrument, settlementDate);

        var expected = Math.Round(2.50m * (90m / 181m), 6);
        result.Should().Be(expected);
    }

    // Accrued income: Corporate bond (quarterly, 30/360)

    [Fact]
    public void CalculateAccruedIncome_CorporateBond_Quarterly_Thirty360_ReturnsExpected()
    {
        // Corporate bond: 6% coupon, quarterly, 30/360, face value 1000
        // Last coupon: 15 Mar 2025, next coupon: 15 Jun 2025
        // Settlement: 15 May 2025
        // Coupon per period = 1000 * (0.06 / 4) = 15.00
        // 30/360: Mar 15 to May 15 = 2 months * 30 = 60 days / 360
        // 30/360: Mar 15 to Jun 15 = 3 months * 30 = 90 days / 360
        // accruedIncome = 15.00 * (60/360) / (90/360) = 15.00 * 60/90 = 10.00
        var details = FixedIncomeDetails.Create(
            couponRate: 0.06m,
            maturityDate: new DateTime(2030, 3, 15),
            couponFrequency: CouponFrequency.Quarterly,
            dayCountConvention: DayCountConvention.Thirty360,
            lastCouponDate: new DateTime(2025, 3, 15),
            faceValue: 1000m);

        var instrument = CreateFixedIncomeInstrument(details);
        var settlementDate = new DateTime(2025, 5, 15);

        var result = _strategy.CalculateAccruedIncome(instrument, settlementDate);

        result.Should().Be(10.00m);
    }

    // Accrued income: Annual bond (ACT/365 Fixed)

    [Fact]
    public void CalculateAccruedIncome_AnnualBond_Act365Fixed_ReturnsExpected()
    {
        // Annual bond: 4% coupon, annual, ACT/365 Fixed, face value 100
        // Last coupon: 15 Jun 2025, next coupon: 15 Jun 2026
        // Settlement: 15 Sep 2025
        // Coupon per period = 100 * (0.04 / 1) = 4.00
        // Accrual: 15 Jun to 15 Sep = 92 days / 365
        // Period: 15 Jun 2025 to 15 Jun 2026 = 365 days / 365
        // accruedIncome = 4.00 * (92/365) / (365/365) = 4.00 * 92/365
        var details = FixedIncomeDetails.Create(
            couponRate: 0.04m,
            maturityDate: new DateTime(2035, 6, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 6, 15),
            faceValue: 100m);

        var instrument = CreateFixedIncomeInstrument(details);
        var settlementDate = new DateTime(2025, 9, 15);

        var result = _strategy.CalculateAccruedIncome(instrument, settlementDate);

        var expected = Math.Round(4.00m * (92m / 365m) / (365m / 365m), 6);
        result.Should().Be(expected);
    }

    // Zero coupon bond

    [Fact]
    public void CalculateAccruedIncome_ZeroCouponBond_ReturnsZero()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 0m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        var instrument = CreateFixedIncomeInstrument(details);
        var settlementDate = new DateTime(2025, 6, 15);

        var result = _strategy.CalculateAccruedIncome(instrument, settlementDate);

        result.Should().Be(0m);
    }

    // Null FixedIncomeDetails

    [Fact]
    public void CalculateAccruedIncome_NullFixedIncomeDetails_ReturnsZero()
    {
        var instrument = CreateFixedIncomeInstrument();
        var settlementDate = new DateTime(2025, 6, 15);

        var result = _strategy.CalculateAccruedIncome(instrument, settlementDate);

        result.Should().Be(0m);
    }

    // Null instrument

    [Fact]
    public void CalculateAccruedIncome_NullInstrument_Throws()
    {
        var act = () => _strategy.CalculateAccruedIncome(null!, new DateTime(2025, 6, 15));

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
    }

    // Settlement on coupon date

    [Fact]
    public void CalculateAccruedIncome_SettlementOnLastCouponDate_ReturnsZero()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2036, 1, 15),
            couponFrequency: CouponFrequency.SemiAnnual,
            dayCountConvention: DayCountConvention.ActualActualIsda,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        var instrument = CreateFixedIncomeInstrument(details);
        var settlementDate = new DateTime(2025, 1, 15);

        var result = _strategy.CalculateAccruedIncome(instrument, settlementDate);

        result.Should().Be(0m);
    }

    // Full period accrual (settlement = next coupon date)

    [Fact]
    public void CalculateAccruedIncome_FullPeriod_ReturnsCouponPerPeriod()
    {
        // Settlement on next coupon date = full period accrual
        // 5% semi-annual, face value 100 => coupon per period = 2.50
        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2036, 1, 15),
            couponFrequency: CouponFrequency.SemiAnnual,
            dayCountConvention: DayCountConvention.ActualActualIsda,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        var instrument = CreateFixedIncomeInstrument(details);
        var settlementDate = new DateTime(2025, 7, 15);

        var result = _strategy.CalculateAccruedIncome(instrument, settlementDate);

        result.Should().Be(2.50m);
    }

    // Monthly coupon

    [Fact]
    public void CalculateAccruedIncome_Monthly_Act365_ReturnsExpected()
    {
        // Monthly: 3% coupon, ACT/365 Fixed, face value 100
        // Last coupon: 15 Jun 2025, next coupon: 15 Jul 2025
        // Settlement: 30 Jun 2025
        // Coupon per period = 100 * (0.03 / 12) = 0.25
        // Accrual: 15 Jun to 30 Jun = 15 days / 365
        // Period: 15 Jun to 15 Jul = 30 days / 365
        // accruedIncome = 0.25 * (15/365) / (30/365) = 0.25 * 15/30 = 0.125
        var details = FixedIncomeDetails.Create(
            couponRate: 0.03m,
            maturityDate: new DateTime(2030, 6, 15),
            couponFrequency: CouponFrequency.Monthly,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 6, 15),
            faceValue: 100m);

        var instrument = CreateFixedIncomeInstrument(details);
        var settlementDate = new DateTime(2025, 6, 30);

        var result = _strategy.CalculateAccruedIncome(instrument, settlementDate);

        result.Should().Be(0.125m);
    }
}
