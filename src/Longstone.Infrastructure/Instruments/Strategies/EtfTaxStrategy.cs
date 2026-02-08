using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.Strategies;

namespace Longstone.Infrastructure.Instruments.Strategies;

public class EtfTaxStrategy : IInstrumentTaxStrategy
{
    public decimal CalculateStampDuty(decimal consideration, Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        return 0m;
    }

    public TaxTreatment GetDividendTaxTreatment(Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        return instrument.CountryOfListing == CountryCodes.UnitedKingdom ? TaxTreatment.Taxable : TaxTreatment.WithholdingTax;
    }
}
