namespace Longstone.Domain.Instruments.Strategies;

public interface IInstrumentValuationStrategy
{
    decimal CalculateMarketValue(decimal quantity, decimal price);

    decimal CalculateAccruedIncome(Instrument instrument, DateTime settlementDate);
}
