using Longstone.Domain.Common;
using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Commands.CreateFund;

public sealed class CreateFundCommandHandler(
    IFundRepository fundRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CreateFundCommand, Guid>
{
    public async Task<Guid> Handle(CreateFundCommand request, CancellationToken cancellationToken)
    {
        var fund = Fund.Create(
            name: request.Name,
            lei: request.Lei,
            isin: request.Isin,
            fundType: request.FundType,
            baseCurrency: request.BaseCurrency,
            benchmarkIndex: request.BenchmarkIndex,
            inceptionDate: request.InceptionDate,
            timeProvider: timeProvider);

        await fundRepository.AddAsync(fund, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return fund.Id;
    }
}
