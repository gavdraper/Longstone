using Longstone.Application.Common.Models;
using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Queries.GetFunds;

public sealed class GetFundsHandler(
    IFundRepository fundRepository) : IRequestHandler<GetFundsQuery, PaginatedList<FundDto>>
{
    public async Task<PaginatedList<FundDto>> Handle(GetFundsQuery request, CancellationToken cancellationToken)
    {
        var criteria = new FundSearchCriteria(
            SearchTerm: request.SearchTerm,
            StatusFilter: request.StatusFilter,
            ManagerFilter: request.ManagerFilter,
            Page: request.Page,
            PageSize: request.PageSize);

        var (items, totalCount) = await fundRepository.SearchAsync(criteria, cancellationToken);

        var dtos = items.Select(FundMapping.ToDto).ToList();

        return new PaginatedList<FundDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
