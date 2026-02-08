namespace Longstone.Domain.Funds;

public sealed record FundSearchCriteria(
    string? SearchTerm = null,
    FundStatus? StatusFilter = null,
    Guid? ManagerFilter = null,
    int Page = 1,
    int PageSize = 20);
