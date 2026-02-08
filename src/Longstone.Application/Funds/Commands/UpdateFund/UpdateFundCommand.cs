using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Commands.UpdateFund;

public sealed record UpdateFundCommand(
    Guid Id,
    string Name,
    string Lei,
    string Isin,
    FundType FundType,
    string BaseCurrency,
    string? BenchmarkIndex,
    DateTime InceptionDate) : IRequest, IFundDetailsCommand;
