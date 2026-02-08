using FluentValidation;

namespace Longstone.Application.Funds.Commands.ChangeFundStatus;

public sealed class ChangeFundStatusValidator : AbstractValidator<ChangeFundStatusCommand>
{
    public ChangeFundStatusValidator()
    {
        RuleFor(x => x.FundId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}
