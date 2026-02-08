using Longstone.Application.Common.Exceptions;
using Longstone.Domain.Common;
using Longstone.Domain.Compliance;
using MediatR;

namespace Longstone.Application.Compliance.Commands.ToggleMandateRule;

public sealed class ToggleMandateRuleCommandHandler(
    IMandateRuleRepository mandateRuleRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<ToggleMandateRuleCommand>
{
    public async Task Handle(ToggleMandateRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await mandateRuleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(MandateRule), request.Id);

        if (request.IsActive)
        {
            rule.Activate(timeProvider);
        }
        else
        {
            rule.Deactivate(timeProvider);
        }

        mandateRuleRepository.Update(rule);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
