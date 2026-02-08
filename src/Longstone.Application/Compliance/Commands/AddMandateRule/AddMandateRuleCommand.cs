using Longstone.Domain.Compliance;
using MediatR;

namespace Longstone.Application.Compliance.Commands.AddMandateRule;

public sealed record AddMandateRuleCommand(
    Guid FundId,
    MandateRuleType RuleType,
    string Parameters,
    RuleSeverity Severity,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo) : IRequest<Guid>;
