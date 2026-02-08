using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Commands.CreateFund;

public sealed record CreateFundCommand(
    string Name,
    string Lei,
    string Isin,
    FundType FundType,
    string BaseCurrency,
    string? BenchmarkIndex,
    DateTime InceptionDate) : IRequest<Guid>, IFundDetailsCommand;
