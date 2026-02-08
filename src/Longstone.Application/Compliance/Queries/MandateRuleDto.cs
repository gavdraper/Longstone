using Longstone.Domain.Compliance;

namespace Longstone.Application.Compliance.Queries;

public sealed record MandateRuleDto(
    Guid Id,
    Guid FundId,
    MandateRuleType RuleType,
    string Parameters,
    RuleSeverity Severity,
    bool IsActive,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    DateTime CreatedAt,
    DateTime UpdatedAt);
