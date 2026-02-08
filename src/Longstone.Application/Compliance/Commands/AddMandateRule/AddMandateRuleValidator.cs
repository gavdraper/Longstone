using FluentValidation;

namespace Longstone.Application.Compliance.Commands.AddMandateRule;

public sealed class AddMandateRuleValidator : AbstractValidator<AddMandateRuleCommand>
{
    public AddMandateRuleValidator()
    {
        RuleFor(x => x.FundId).NotEmpty();
        RuleFor(x => x.RuleType).IsInEnum();
        RuleFor(x => x.Parameters).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective to date cannot be before effective from date.");
    }
}
