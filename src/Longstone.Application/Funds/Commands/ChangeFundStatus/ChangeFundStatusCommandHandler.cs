using Longstone.Application.Common.Exceptions;
using Longstone.Domain.Common;
using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Commands.ChangeFundStatus;

public sealed class ChangeFundStatusCommandHandler(
    IFundRepository fundRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<ChangeFundStatusCommand>
{
    public async Task Handle(ChangeFundStatusCommand request, CancellationToken cancellationToken)
    {
        var fund = await fundRepository.GetByIdAsync(request.FundId, cancellationToken)
            ?? throw new NotFoundException(nameof(Fund), request.FundId);

        switch (request.NewStatus)
        {
            case FundStatus.Active:
                fund.Reactivate(timeProvider);
                break;
            case FundStatus.Suspended:
                fund.Suspend(timeProvider);
                break;
            case FundStatus.Closed:
                fund.Close(timeProvider);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.NewStatus), request.NewStatus, "Invalid fund status.");
        }

        fundRepository.Update(fund);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
