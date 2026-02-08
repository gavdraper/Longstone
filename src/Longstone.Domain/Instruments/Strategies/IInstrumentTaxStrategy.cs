namespace Longstone.Domain.Instruments.Strategies;

public interface IInstrumentTaxStrategy
{
    decimal CalculateStampDuty(decimal consideration, Instrument instrument);

    TaxTreatment GetDividendTaxTreatment(Instrument instrument);
}
