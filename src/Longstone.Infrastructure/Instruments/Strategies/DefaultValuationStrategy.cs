using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.Strategies;

namespace Longstone.Infrastructure.Instruments.Strategies;

public class DefaultValuationStrategy : IInstrumentValuationStrategy
{
    public decimal CalculateMarketValue(decimal quantity, decimal price)
    {
        return quantity * price;
    }

    public decimal CalculateAccruedIncome(Instrument instrument, DateTime settlementDate)
    {
        return 0m;
    }
}
