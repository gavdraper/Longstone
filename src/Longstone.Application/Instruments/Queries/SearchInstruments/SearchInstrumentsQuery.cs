using Longstone.Application.Common.Models;
using Longstone.Domain.Instruments;
using MediatR;

namespace Longstone.Application.Instruments.Queries.SearchInstruments;

public sealed record SearchInstrumentsQuery(
    string? SearchTerm = null,
    AssetClass? AssetClassFilter = null,
    Exchange? ExchangeFilter = null,
    string? CountryFilter = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedList<InstrumentDto>>;
