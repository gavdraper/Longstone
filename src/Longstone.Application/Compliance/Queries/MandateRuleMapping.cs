using Longstone.Domain.Compliance;

namespace Longstone.Application.Compliance.Queries;

internal static class MandateRuleMapping
{
    public static MandateRuleDto ToDto(MandateRule rule) =>
        new(
            Id: rule.Id,
            FundId: rule.FundId,
            RuleType: rule.RuleType,
            Parameters: rule.Parameters,
            Severity: rule.Severity,
            IsActive: rule.IsActive,
            EffectiveFrom: rule.EffectiveFrom,
            EffectiveTo: rule.EffectiveTo,
            CreatedAt: rule.CreatedAt,
            UpdatedAt: rule.UpdatedAt);
}
