using Longstone.Domain.Funds;

namespace Longstone.Application.Funds.Queries;

internal static class FundMapping
{
    public static FundDto ToDto(Fund fund) =>
        new(
            Id: fund.Id,
            Name: fund.Name,
            Lei: fund.Lei,
            Isin: fund.Isin,
            FundType: fund.FundType,
            BaseCurrency: fund.BaseCurrency,
            BenchmarkIndex: fund.BenchmarkIndex,
            InceptionDate: fund.InceptionDate,
            Status: fund.Status,
            Managers: fund.AssignedManagers
                .Select(m => new FundManagerDto(m.UserId))
                .ToList(),
            CreatedAt: fund.CreatedAt,
            UpdatedAt: fund.UpdatedAt);
}
