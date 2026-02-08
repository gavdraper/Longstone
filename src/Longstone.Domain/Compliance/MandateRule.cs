using Longstone.Domain.Common;

namespace Longstone.Domain.Compliance;

public class MandateRule : IAuditable
{
    public Guid Id { get; private set; }
    public Guid FundId { get; private set; }
    public MandateRuleType RuleType { get; private set; }
    public string Parameters { get; private set; } = string.Empty;
    public RuleSeverity Severity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private MandateRule() { }

    public static MandateRule Create(
        Guid fundId,
        MandateRuleType ruleType,
        string parameters,
        RuleSeverity severity,
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        TimeProvider timeProvider)
    {
        if (fundId == Guid.Empty)
        {
            throw new ArgumentException("Fund ID cannot be empty.", nameof(fundId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(parameters);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (!Enum.IsDefined(ruleType))
            throw new ArgumentOutOfRangeException(nameof(ruleType));
        if (!Enum.IsDefined(severity))
            throw new ArgumentOutOfRangeException(nameof(severity));

        ValidateEffectiveDateRange(effectiveFrom, effectiveTo);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        return new MandateRule
        {
            Id = Guid.NewGuid(),
            FundId = fundId,
            RuleType = ruleType,
            Parameters = parameters,
            Severity = severity,
            IsActive = true,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Deactivate(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        IsActive = false;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Activate(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        IsActive = true;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void UpdateEffectiveDates(DateTime effectiveFrom, DateTime? effectiveTo, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        ValidateEffectiveDateRange(effectiveFrom, effectiveTo);

        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    private static void ValidateEffectiveDateRange(DateTime effectiveFrom, DateTime? effectiveTo)
    {
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            throw new ArgumentException("Effective to date cannot be before effective from date.", nameof(effectiveTo));
        }
    }
}
