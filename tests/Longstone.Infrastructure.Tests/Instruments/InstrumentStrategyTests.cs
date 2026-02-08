using FluentAssertions;
using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.Strategies;
using Longstone.Infrastructure.Instruments.Strategies;
using Microsoft.Extensions.Time.Testing;

namespace Longstone.Infrastructure.Tests.Instruments;

public class InstrumentStrategyTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private Instrument CreateInstrument(
        Exchange exchange = Exchange.LSE,
        string countryOfListing = "GB",
        AssetClass assetClass = AssetClass.Equity,
        InstrumentStatus? status = null)
    {
        var instrument = Instrument.Create(
            "GB0007188757", "0718875", "RIO", exchange,
            "Rio Tinto plc", "GBP", countryOfListing, "Basic Materials",
            assetClass, 50_000_000_000m, _timeProvider);

        if (status == InstrumentStatus.Suspended)
        {
            instrument.Suspend(_timeProvider);
        }
        else if (status == InstrumentStatus.Delisted)
        {
            instrument.Delist(_timeProvider);
        }

        return instrument;
    }

    // Default valuation strategy tests

    public class DefaultValuationStrategyTests : InstrumentStrategyTests
    {
        private readonly DefaultValuationStrategy _strategy = new();

        [Fact]
        public void CalculateMarketValue_ReturnsQuantityTimesPrice()
        {
            var result = _strategy.CalculateMarketValue(100m, 45.50m);

            result.Should().Be(4550m);
        }

        [Fact]
        public void CalculateMarketValue_WithZeroQuantity_ReturnsZero()
        {
            var result = _strategy.CalculateMarketValue(0m, 45.50m);

            result.Should().Be(0m);
        }

        [Fact]
        public void CalculateMarketValue_WithFractionalShares_ReturnsCorrectValue()
        {
            var result = _strategy.CalculateMarketValue(1.5m, 100m);

            result.Should().Be(150m);
        }

        [Fact]
        public void CalculateAccruedIncome_ReturnsZero()
        {
            var instrument = CreateInstrument();

            var result = _strategy.CalculateAccruedIncome(instrument, _timeProvider.GetUtcNow().UtcDateTime);

            result.Should().Be(0m);
        }
    }

    // Equity tax strategy tests

    public class EquityTaxStrategyTests : InstrumentStrategyTests
    {
        private readonly EquityTaxStrategy _strategy = new();

        [Fact]
        public void CalculateStampDuty_UkMainMarket_AppliesHalfPercent()
        {
            var instrument = CreateInstrument(exchange: Exchange.LSE, countryOfListing: "GB");

            var result = _strategy.CalculateStampDuty(10_000m, instrument);

            result.Should().Be(50m);
        }

        [Fact]
        public void CalculateStampDuty_UkMainMarket_RoundsToTwoDecimalPlaces()
        {
            var instrument = CreateInstrument(exchange: Exchange.LSE, countryOfListing: "GB");

            var result = _strategy.CalculateStampDuty(10_001m, instrument);

            result.Should().Be(50.01m);
        }

        [Fact]
        public void CalculateStampDuty_NonUkExchange_ReturnsZero()
        {
            var instrument = CreateInstrument(exchange: Exchange.NYSE, countryOfListing: "US");

            var result = _strategy.CalculateStampDuty(10_000m, instrument);

            result.Should().Be(0m);
        }

        [Fact]
        public void CalculateStampDuty_UkExchangeNonGbListing_ReturnsZero()
        {
            var instrument = CreateInstrument(exchange: Exchange.LSE, countryOfListing: "IE");

            var result = _strategy.CalculateStampDuty(10_000m, instrument);

            result.Should().Be(0m);
        }

        [Fact]
        public void GetDividendTaxTreatment_ReturnsAlwaysTaxable()
        {
            var instrument = CreateInstrument();

            var result = _strategy.GetDividendTaxTreatment(instrument);

            result.Should().Be(TaxTreatment.Taxable);
        }

        [Fact]
        public void CalculateStampDuty_WithNullInstrument_Throws()
        {
            var act = () => _strategy.CalculateStampDuty(10_000m, null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
        }

        [Fact]
        public void GetDividendTaxTreatment_WithNullInstrument_Throws()
        {
            var act = () => _strategy.GetDividendTaxTreatment(null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
        }
    }

    // ETF tax strategy tests

    public class EtfTaxStrategyTests : InstrumentStrategyTests
    {
        private readonly EtfTaxStrategy _strategy = new();

        [Fact]
        public void CalculateStampDuty_AlwaysReturnsZero()
        {
            var instrument = CreateInstrument(exchange: Exchange.LSE, countryOfListing: "GB", assetClass: AssetClass.ETF);

            var result = _strategy.CalculateStampDuty(10_000m, instrument);

            result.Should().Be(0m);
        }

        [Fact]
        public void CalculateStampDuty_NonUk_ReturnsZero()
        {
            var instrument = CreateInstrument(exchange: Exchange.NYSE, countryOfListing: "US", assetClass: AssetClass.ETF);

            var result = _strategy.CalculateStampDuty(10_000m, instrument);

            result.Should().Be(0m);
        }

        [Fact]
        public void GetDividendTaxTreatment_UkEtf_ReturnsTaxable()
        {
            var instrument = CreateInstrument(countryOfListing: "GB", assetClass: AssetClass.ETF);

            var result = _strategy.GetDividendTaxTreatment(instrument);

            result.Should().Be(TaxTreatment.Taxable);
        }

        [Fact]
        public void GetDividendTaxTreatment_NonUkEtf_ReturnsWithholdingTax()
        {
            var instrument = CreateInstrument(countryOfListing: "US", assetClass: AssetClass.ETF);

            var result = _strategy.GetDividendTaxTreatment(instrument);

            result.Should().Be(TaxTreatment.WithholdingTax);
        }

        [Fact]
        public void CalculateStampDuty_WithNullInstrument_Throws()
        {
            var act = () => _strategy.CalculateStampDuty(10_000m, null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
        }

        [Fact]
        public void GetDividendTaxTreatment_WithNullInstrument_Throws()
        {
            var act = () => _strategy.GetDividendTaxTreatment(null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
        }
    }

    // NotSupported tax strategy tests

    public class NotSupportedTaxStrategyTests : InstrumentStrategyTests
    {
        private readonly NotSupportedTaxStrategy _strategy = new();

        [Fact]
        public void CalculateStampDuty_ThrowsNotSupported()
        {
            var instrument = CreateInstrument(assetClass: AssetClass.FixedIncome);

            var act = () => _strategy.CalculateStampDuty(10_000m, instrument);

            act.Should().Throw<NotSupportedException>()
                .WithMessage("*FixedIncome*");
        }

        [Fact]
        public void GetDividendTaxTreatment_ThrowsNotSupported()
        {
            var instrument = CreateInstrument(assetClass: AssetClass.FixedIncome);

            var act = () => _strategy.GetDividendTaxTreatment(instrument);

            act.Should().Throw<NotSupportedException>()
                .WithMessage("*FixedIncome*");
        }

        [Fact]
        public void CalculateStampDuty_WithNullInstrument_Throws()
        {
            var act = () => _strategy.CalculateStampDuty(10_000m, null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
        }

        [Fact]
        public void GetDividendTaxTreatment_WithNullInstrument_Throws()
        {
            var act = () => _strategy.GetDividendTaxTreatment(null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
        }
    }

    // Default compliance strategy tests

    public class DefaultComplianceStrategyTests : InstrumentStrategyTests
    {
        private readonly DefaultComplianceStrategy _strategy = new();

        [Fact]
        public void IsEligibleForFund_ActiveInstrument_ReturnsTrue()
        {
            var instrument = CreateInstrument();

            var result = _strategy.IsEligibleForFund(instrument);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsEligibleForFund_SuspendedInstrument_ReturnsFalse()
        {
            var instrument = CreateInstrument(status: InstrumentStatus.Suspended);

            var result = _strategy.IsEligibleForFund(instrument);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsEligibleForFund_DelistedInstrument_ReturnsFalse()
        {
            var instrument = CreateInstrument(status: InstrumentStatus.Delisted);

            var result = _strategy.IsEligibleForFund(instrument);

            result.Should().BeFalse();
        }

        [Fact]
        public void GetComplianceFlags_ActiveInstrument_ReturnsEmpty()
        {
            var instrument = CreateInstrument();

            var result = _strategy.GetComplianceFlags(instrument);

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetComplianceFlags_SuspendedInstrument_ReturnsSuspendedFlag()
        {
            var instrument = CreateInstrument(status: InstrumentStatus.Suspended);

            var result = _strategy.GetComplianceFlags(instrument);

            result.Should().ContainSingle().Which.Should().Be("SUSPENDED");
        }

        [Fact]
        public void GetComplianceFlags_DelistedInstrument_ReturnsDelistedFlag()
        {
            var instrument = CreateInstrument(status: InstrumentStatus.Delisted);

            var result = _strategy.GetComplianceFlags(instrument);

            result.Should().ContainSingle().Which.Should().Be("DELISTED");
        }

        [Fact]
        public void IsEligibleForFund_WithNullInstrument_Throws()
        {
            var act = () => _strategy.IsEligibleForFund(null!);

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
        }

        [Fact]
        public void GetComplianceFlags_WithNullInstrument_Throws()
        {
            var act = () => _strategy.GetComplianceFlags(null!).ToList();

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instrument");
        }
    }
}
