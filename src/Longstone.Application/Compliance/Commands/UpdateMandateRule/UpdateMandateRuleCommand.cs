using Longstone.Domain.Compliance;
using MediatR;

namespace Longstone.Application.Compliance.Commands.UpdateMandateRule;

public sealed record UpdateMandateRuleCommand(
    Guid Id,
    string Parameters,
    RuleSeverity Severity,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo) : IRequest;
