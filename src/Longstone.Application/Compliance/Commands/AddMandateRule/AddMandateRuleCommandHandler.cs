using Longstone.Domain.Common;
using Longstone.Domain.Compliance;
using MediatR;

namespace Longstone.Application.Compliance.Commands.AddMandateRule;

public sealed class AddMandateRuleCommandHandler(
    IMandateRuleRepository mandateRuleRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<AddMandateRuleCommand, Guid>
{
    public async Task<Guid> Handle(AddMandateRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = MandateRule.Create(
            fundId: request.FundId,
            ruleType: request.RuleType,
            parameters: request.Parameters,
            severity: request.Severity,
            effectiveFrom: request.EffectiveFrom,
            effectiveTo: request.EffectiveTo,
            timeProvider: timeProvider);

        await mandateRuleRepository.AddAsync(rule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }
}
