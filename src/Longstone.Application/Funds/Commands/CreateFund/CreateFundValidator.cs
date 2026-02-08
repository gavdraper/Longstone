using FluentValidation;

namespace Longstone.Application.Funds.Commands.CreateFund;

public sealed class CreateFundValidator : AbstractValidator<CreateFundCommand>
{
    public CreateFundValidator()
    {
        Include(new FundDetailsValidator<CreateFundCommand>());
    }
}
