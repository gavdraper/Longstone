using FluentAssertions;
using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.DayCount;

namespace Longstone.Domain.Tests.Instruments;

public class DayCountCalculatorFactoryTests
{
    [Fact]
    public void Create_ActualActualIsda_ReturnsActualActualIsdaCalculator()
    {
        var calculator = DayCountCalculatorFactory.Create(DayCountConvention.ActualActualIsda);

        calculator.Should().BeOfType<ActualActualIsdaCalculator>();
    }

    [Fact]
    public void Create_Actual365Fixed_ReturnsActual365FixedCalculator()
    {
        var calculator = DayCountCalculatorFactory.Create(DayCountConvention.Actual365Fixed);

        calculator.Should().BeOfType<Actual365FixedCalculator>();
    }

    [Fact]
    public void Create_Thirty360_ReturnsThirty360Calculator()
    {
        var calculator = DayCountCalculatorFactory.Create(DayCountConvention.Thirty360);

        calculator.Should().BeOfType<Thirty360Calculator>();
    }

    [Fact]
    public void Create_InvalidConvention_ThrowsArgumentOutOfRangeException()
    {
        var act = () => DayCountCalculatorFactory.Create((DayCountConvention)999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
