using Longstone.Application.Common.Exceptions;
using Longstone.Domain.Common;
using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Commands.UpdateFund;

public sealed class UpdateFundCommandHandler(
    IFundRepository fundRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<UpdateFundCommand>
{
    public async Task Handle(UpdateFundCommand request, CancellationToken cancellationToken)
    {
        var fund = await fundRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Fund), request.Id);

        fund.UpdateDetails(
            name: request.Name,
            lei: request.Lei,
            isin: request.Isin,
            fundType: request.FundType,
            baseCurrency: request.BaseCurrency,
            benchmarkIndex: request.BenchmarkIndex,
            inceptionDate: request.InceptionDate,
            timeProvider: timeProvider);

        fundRepository.Update(fund);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
