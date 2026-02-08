using Longstone.Domain.Instruments;
using Longstone.Domain.Instruments.Strategies;

namespace Longstone.Infrastructure.Instruments.Strategies;

public class DefaultComplianceStrategy : IInstrumentComplianceStrategy
{
    public bool IsEligibleForFund(Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        return instrument.Status == InstrumentStatus.Active;
    }

    public IEnumerable<string> GetComplianceFlags(Instrument instrument)
    {
        ArgumentNullException.ThrowIfNull(instrument);

        if (instrument.Status == InstrumentStatus.Suspended)
        {
            yield return "SUSPENDED";
        }

        if (instrument.Status == InstrumentStatus.Delisted)
        {
            yield return "DELISTED";
        }
    }
}
