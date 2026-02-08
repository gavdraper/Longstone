using FluentValidation;

namespace Longstone.Application.Funds.Commands;

internal sealed class FundDetailsValidator<T> : AbstractValidator<T>
    where T : IFundDetailsCommand
{
    public FundDetailsValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Lei).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Isin).NotEmpty().MaximumLength(12);
        RuleFor(x => x.FundType).IsInEnum();
        RuleFor(x => x.BaseCurrency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.BenchmarkIndex).MaximumLength(200);
        RuleFor(x => x.InceptionDate).NotEmpty();
    }
}
