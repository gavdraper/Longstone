namespace Longstone.Domain.Instruments.Strategies;

public interface IInstrumentComplianceStrategy
{
    bool IsEligibleForFund(Instrument instrument);

    IEnumerable<string> GetComplianceFlags(Instrument instrument);
}
