using Longstone.Domain.Funds;

namespace Longstone.Application.Funds.Queries;

public sealed record FundDto(
    Guid Id,
    string Name,
    string Lei,
    string Isin,
    FundType FundType,
    string BaseCurrency,
    string? BenchmarkIndex,
    DateTime InceptionDate,
    FundStatus Status,
    IReadOnlyList<FundManagerDto> Managers,
    DateTime CreatedAt,
    DateTime UpdatedAt);
