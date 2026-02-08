using MediatR;

namespace Longstone.Application.Funds.Queries.GetFundById;

public sealed record GetFundByIdQuery(Guid Id) : IRequest<FundDto?>;
