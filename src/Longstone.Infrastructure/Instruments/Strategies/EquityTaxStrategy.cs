using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.Strategies;

namespace Longstone.Infrastructure.Instruments.Strategies;

public class EquityTaxStrategy : IInstrumentTaxStrategy
{
    private const decimal UkStampDutyRate = 0.005m;

    public decimal CalculateStampDuty(decimal consideration, Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        if (IsUkMainMarket(instrument))
        {
            return Math.Round(consideration * UkStampDutyRate, 2, MidpointRounding.AwayFromZero);
        }

        return 0m;
    }

    public TaxTreatment GetDividendTaxTreatment(Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        return TaxTreatment.Taxable;
    }

    private static bool IsUkMainMarket(Instrument instrument)
    {
        return instrument.Exchange == Exchange.LSE && instrument.CountryOfListing == CountryCodes.UnitedKingdom;
    }
}
