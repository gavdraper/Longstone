using FluentValidation;

namespace Longstone.Application.Compliance.Commands.ToggleMandateRule;

public sealed class ToggleMandateRuleValidator : AbstractValidator<ToggleMandateRuleCommand>
{
    public ToggleMandateRuleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
