using Longstone.Domain.Funds;

namespace Longstone.Application.Funds.Commands;

public interface IFundDetailsCommand
{
    string Name { get; }
    string Lei { get; }
    string Isin { get; }
    FundType FundType { get; }
    string BaseCurrency { get; }
    string? BenchmarkIndex { get; }
    DateTime InceptionDate { get; }
}
