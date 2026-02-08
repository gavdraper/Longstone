using FluentAssertions;
using Longstone.Domain.Instruments;

namespace Longstone.Domain.Tests.Instruments;

public class FixedIncomeDetailsTests
{
    [Fact]
    public void Create_WithValidInputs_SetsAllProperties()
    {
        var lastCouponDate = new DateTime(2025, 1, 15);
        var maturityDate = new DateTime(2035, 1, 15);

        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: maturityDate,
            couponFrequency: CouponFrequency.SemiAnnual,
            dayCountConvention: DayCountConvention.ActualActualIsda,
            lastCouponDate: lastCouponDate,
            faceValue: 100m);

        details.CouponRate.Should().Be(0.05m);
        details.MaturityDate.Should().Be(maturityDate);
        details.CouponFrequency.Should().Be(CouponFrequency.SemiAnnual);
        details.DayCountConvention.Should().Be(DayCountConvention.ActualActualIsda);
        details.LastCouponDate.Should().Be(lastCouponDate);
        details.FaceValue.Should().Be(100m);
    }

    [Fact]
    public void Create_WithZeroCouponRate_Succeeds()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 0m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        details.CouponRate.Should().Be(0m);
    }

    [Fact]
    public void Create_WithCouponRateOfOne_Succeeds()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 1m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        details.CouponRate.Should().Be(1m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(-1)]
    [InlineData(2)]
    public void Create_WithCouponRateOutOfRange_Throws(decimal couponRate)
    {
        var act = () => FixedIncomeDetails.Create(
            couponRate: couponRate,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("couponRate");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidFaceValue_Throws(decimal faceValue)
    {
        var act = () => FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: faceValue);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("faceValue");
    }

    [Fact]
    public void Create_WithMaturityBeforeLastCoupon_Throws()
    {
        var act = () => FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2024, 1, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(29)]
    [InlineData(30)]
    [InlineData(31)]
    public void Create_WithLastCouponDateDayAfter28_Throws(int day)
    {
        var act = () => FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, day),
            faceValue: 100m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("lastCouponDate");
    }

    [Fact]
    public void Create_WithLastCouponDateDay28_Succeeds()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2035, 1, 28),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, 28),
            faceValue: 100m);

        details.LastCouponDate.Day.Should().Be(28);
    }

    // NextCouponDate tests

    [Fact]
    public void NextCouponDate_Annual_Returns12MonthsAfterLastCoupon()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Annual,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        details.NextCouponDate.Should().Be(new DateTime(2026, 1, 15));
    }

    [Fact]
    public void NextCouponDate_SemiAnnual_Returns6MonthsAfterLastCoupon()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.SemiAnnual,
            dayCountConvention: DayCountConvention.ActualActualIsda,
            lastCouponDate: new DateTime(2025, 1, 15),
            faceValue: 100m);

        details.NextCouponDate.Should().Be(new DateTime(2025, 7, 15));
    }

    [Fact]
    public void NextCouponDate_Quarterly_Returns3MonthsAfterLastCoupon()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Quarterly,
            dayCountConvention: DayCountConvention.Thirty360,
            lastCouponDate: new DateTime(2025, 3, 15),
            faceValue: 100m);

        details.NextCouponDate.Should().Be(new DateTime(2025, 6, 15));
    }

    [Fact]
    public void NextCouponDate_Monthly_Returns1MonthAfterLastCoupon()
    {
        var details = FixedIncomeDetails.Create(
            couponRate: 0.05m,
            maturityDate: new DateTime(2035, 1, 15),
            couponFrequency: CouponFrequency.Monthly,
            dayCountConvention: DayCountConvention.Actual365Fixed,
            lastCouponDate: new DateTime(2025, 6, 15),
            faceValue: 100m);

        details.NextCouponDate.Should().Be(new DateTime(2025, 7, 15));
    }

    // Value equality (sealed record)

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var details1 = FixedIncomeDetails.Create(0.05m, new DateTime(2035, 1, 15), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 1, 15), 100m);
        var details2 = FixedIncomeDetails.Create(0.05m, new DateTime(2035, 1, 15), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 1, 15), 100m);

        details1.Should().Be(details2);
    }

    [Fact]
    public void Equals_DifferentCouponRate_ReturnsFalse()
    {
        var details1 = FixedIncomeDetails.Create(0.05m, new DateTime(2035, 1, 15), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 1, 15), 100m);
        var details2 = FixedIncomeDetails.Create(0.06m, new DateTime(2035, 1, 15), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 1, 15), 100m);

        details1.Should().NotBe(details2);
    }
}
