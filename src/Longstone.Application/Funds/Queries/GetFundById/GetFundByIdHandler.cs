using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Queries.GetFundById;

public sealed class GetFundByIdHandler(
    IFundRepository fundRepository) : IRequestHandler<GetFundByIdQuery, FundDto?>
{
    public async Task<FundDto?> Handle(GetFundByIdQuery request, CancellationToken cancellationToken)
    {
        var fund = await fundRepository.GetByIdAsync(request.Id, cancellationToken);

        return fund is null ? null : FundMapping.ToDto(fund);
    }
}
