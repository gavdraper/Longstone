using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.Strategies;

namespace Longstone.Infrastructure.Instruments.Strategies;

public class NotSupportedTaxStrategy : IInstrumentTaxStrategy
{
    public decimal CalculateStampDuty(decimal consideration, Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        throw new NotSupportedException($"Tax strategy is not yet implemented for asset class '{instrument.AssetClass}'.");
    }

    public TaxTreatment GetDividendTaxTreatment(Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        throw new NotSupportedException($"Tax strategy is not yet implemented for asset class '{instrument.AssetClass}'.");
    }
}
