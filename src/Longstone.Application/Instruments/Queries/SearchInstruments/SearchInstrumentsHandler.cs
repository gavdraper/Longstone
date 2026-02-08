using Longstone.Application.Common.Models;
using Longstone.Domain.Instruments;
using MediatR;

namespace Longstone.Application.Instruments.Queries.SearchInstruments;

public sealed class SearchInstrumentsHandler(
    IInstrumentRepository instrumentRepository) : IRequestHandler<SearchInstrumentsQuery, PaginatedList<InstrumentDto>>
{
    public async Task<PaginatedList<InstrumentDto>> Handle(SearchInstrumentsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await instrumentRepository.SearchAsync(
            request.SearchTerm,
            request.AssetClassFilter,
            request.ExchangeFilter,
            request.CountryFilter,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(InstrumentMapping.ToDto).ToList();

        return new PaginatedList<InstrumentDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
