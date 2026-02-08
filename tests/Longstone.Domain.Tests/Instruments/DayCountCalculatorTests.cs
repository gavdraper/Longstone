using FluentAssertions;
using Longstone.Domain.Instruments.DayCount;

namespace Longstone.Domain.Tests.Instruments;

public class DayCountCalculatorTests
{
    public class ActualActualIsdaCalculatorTests
    {
        private readonly ActualActualIsdaCalculator _calculator = new();

        [Fact]
        public void CalculateYearFraction_SameDate_ReturnsZero()
        {
            var date = new DateTime(2025, 6, 15);

            var result = _calculator.CalculateYearFraction(date, date);

            result.Should().Be(0m);
        }

        [Fact]
        public void CalculateYearFraction_FullNonLeapYear_ReturnsOne()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2026, 1, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(1m);
        }

        [Fact]
        public void CalculateYearFraction_FullLeapYear_ReturnsOne()
        {
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2025, 1, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(1m);
        }

        [Fact]
        public void CalculateYearFraction_HalfYearNonLeap_ReturnsCorrectFraction()
        {
            // 1 Jan to 1 Jul 2025 = 181 days in a 365-day year
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 7, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().BeApproximately(181m / 365m, 0.000001m);
        }

        [Fact]
        public void CalculateYearFraction_SpanningLeapYearBoundary_HandlesCorrectly()
        {
            // 1 Nov 2023 (non-leap) to 1 Mar 2024 (leap year)
            // Nov 2023: 30 days remaining (Nov 1 to Dec 1 = 30 days)
            // Dec 2023: 31 days
            // Total 2023 days: 61, year has 365 days -> 61/365
            // Jan 2024: 31 days
            // Feb 2024: 29 days (leap)
            // Total 2024 days: 60, year has 366 days -> 60/366
            var start = new DateTime(2023, 11, 1);
            var end = new DateTime(2024, 3, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            var expected = (61m / 365m) + (60m / 366m);
            result.Should().BeApproximately(expected, 0.000001m);
        }

        [Fact]
        public void CalculateYearFraction_WithinLeapYear_UsesLeapYearDenominator()
        {
            // 1 Jan to 1 Mar 2024 (leap year) = 60 days / 366
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 3, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().BeApproximately(60m / 366m, 0.000001m);
        }

        [Fact]
        public void CalculateYearFraction_StartAfterEnd_ThrowsArgumentException()
        {
            var start = new DateTime(2025, 6, 15);
            var end = new DateTime(2025, 6, 14);

            var act = () => _calculator.CalculateYearFraction(start, end);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CalculateYearFraction_UkGiltTypicalPeriod_ReturnsExpected()
        {
            // UK gilt semi-annual: 15 Jan to 15 Jul 2025 = 181 days / 365
            var start = new DateTime(2025, 1, 15);
            var end = new DateTime(2025, 7, 15);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().BeApproximately(181m / 365m, 0.000001m);
        }
    }

    public class Actual365FixedCalculatorTests
    {
        private readonly Actual365FixedCalculator _calculator = new();

        [Fact]
        public void CalculateYearFraction_SameDate_ReturnsZero()
        {
            var date = new DateTime(2025, 6, 15);

            var result = _calculator.CalculateYearFraction(date, date);

            result.Should().Be(0m);
        }

        [Fact]
        public void CalculateYearFraction_FullNonLeapYear_ReturnsOne()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2026, 1, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(365m / 365m);
        }

        [Fact]
        public void CalculateYearFraction_FullLeapYear_ReturnsGreaterThanOne()
        {
            // 2024 is leap year, 366 days / 365 = 1.002739...
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2025, 1, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(366m / 365m);
        }

        [Fact]
        public void CalculateYearFraction_90Days_ReturnsCorrectFraction()
        {
            // 1 Jan to 1 Apr 2025 = 90 days
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 4, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(90m / 365m);
        }

        [Fact]
        public void CalculateYearFraction_AlwaysDivideBy365_EvenInLeapYear()
        {
            // Feb in leap year: 1 Feb to 1 Mar 2024 = 29 days / 365
            var start = new DateTime(2024, 2, 1);
            var end = new DateTime(2024, 3, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(29m / 365m);
        }

        [Fact]
        public void CalculateYearFraction_StartAfterEnd_ThrowsArgumentException()
        {
            var start = new DateTime(2025, 6, 15);
            var end = new DateTime(2025, 6, 14);

            var act = () => _calculator.CalculateYearFraction(start, end);

            act.Should().Throw<ArgumentException>();
        }
    }

    public class Thirty360CalculatorTests
    {
        private readonly Thirty360Calculator _calculator = new();

        [Fact]
        public void CalculateYearFraction_SameDate_ReturnsZero()
        {
            var date = new DateTime(2025, 6, 15);

            var result = _calculator.CalculateYearFraction(date, date);

            result.Should().Be(0m);
        }

        [Fact]
        public void CalculateYearFraction_FullYear_ReturnsOne()
        {
            var start = new DateTime(2025, 1, 15);
            var end = new DateTime(2026, 1, 15);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(1m);
        }

        [Fact]
        public void CalculateYearFraction_SixMonths_ReturnsHalf()
        {
            // 30/360: 15 Jan to 15 Jul = 6 months * 30 = 180 days / 360
            var start = new DateTime(2025, 1, 15);
            var end = new DateTime(2025, 7, 15);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(180m / 360m);
        }

        [Fact]
        public void CalculateYearFraction_OneMonth_Returns30Over360()
        {
            var start = new DateTime(2025, 3, 1);
            var end = new DateTime(2025, 4, 1);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(30m / 360m);
        }

        [Fact]
        public void CalculateYearFraction_EndOfMonth31_ClampedTo30()
        {
            // 30/360 rule: day 31 -> 30 (with conditions)
            // 31 Jan to 28 Feb: D1=31->30, D2=28
            // = (0*360 + (2-1)*30 + (28-30)) / 360 = (0 + 30 - 2) / 360 = 28/360
            var start = new DateTime(2025, 1, 31);
            var end = new DateTime(2025, 2, 28);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(28m / 360m);
        }

        [Fact]
        public void CalculateYearFraction_BothDay31_BothClamped()
        {
            // 31 Jan to 31 Mar: D1=31->30, D2=31->30 (because D1 was 31)
            // = (0*360 + (3-1)*30 + (30-30)) / 360 = 60/360
            var start = new DateTime(2025, 1, 31);
            var end = new DateTime(2025, 3, 31);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(60m / 360m);
        }

        [Fact]
        public void CalculateYearFraction_QuarterlyPeriod_Returns90Over360()
        {
            // 15 Mar to 15 Jun = 3 months = 90/360
            var start = new DateTime(2025, 3, 15);
            var end = new DateTime(2025, 6, 15);

            var result = _calculator.CalculateYearFraction(start, end);

            result.Should().Be(90m / 360m);
        }

        [Fact]
        public void CalculateYearFraction_StartAfterEnd_ThrowsArgumentException()
        {
            var start = new DateTime(2025, 6, 15);
            var end = new DateTime(2025, 6, 14);

            var act = () => _calculator.CalculateYearFraction(start, end);

            act.Should().Throw<ArgumentException>();
        }
    }
}
