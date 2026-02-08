using Longstone.Application.Common.Models;
using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Queries.GetFunds;

public sealed record GetFundsQuery(
    int Page = 1,
    int PageSize = 20,
    FundStatus? StatusFilter = null,
    string? SearchTerm = null,
    Guid? ManagerFilter = null) : IRequest<PaginatedList<FundDto>>;
