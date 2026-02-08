using FluentValidation;

namespace Longstone.Application.Funds.Commands.UpdateFund;

public sealed class UpdateFundValidator : AbstractValidator<UpdateFundCommand>
{
    public UpdateFundValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new FundDetailsValidator<UpdateFundCommand>());
    }
}
