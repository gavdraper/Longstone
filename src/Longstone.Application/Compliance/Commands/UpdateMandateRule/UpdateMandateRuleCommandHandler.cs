using Longstone.Application.Common.Exceptions;
using Longstone.Domain.Common;
using Longstone.Domain.Compliance;
using MediatR;

namespace Longstone.Application.Compliance.Commands.UpdateMandateRule;

public sealed class UpdateMandateRuleCommandHandler(
    IMandateRuleRepository mandateRuleRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<UpdateMandateRuleCommand>
{
    public async Task Handle(UpdateMandateRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await mandateRuleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(MandateRule), request.Id);

        rule.UpdateDetails(
            parameters: request.Parameters,
            severity: request.Severity,
            effectiveFrom: request.EffectiveFrom,
            effectiveTo: request.EffectiveTo,
            timeProvider: timeProvider);

        mandateRuleRepository.Update(rule);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
