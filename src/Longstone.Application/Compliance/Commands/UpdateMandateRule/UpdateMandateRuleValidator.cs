using FluentValidation;

namespace Longstone.Application.Compliance.Commands.UpdateMandateRule;

public sealed class UpdateMandateRuleValidator : AbstractValidator<UpdateMandateRuleCommand>
{
    public UpdateMandateRuleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Parameters).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.EffectiveFrom).NotEmpty();
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective to date cannot be before effective from date.");
    }
}
