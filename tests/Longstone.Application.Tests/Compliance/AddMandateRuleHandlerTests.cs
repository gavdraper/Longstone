using FluentAssertions;
using Longstone.Application.Compliance.Commands.AddMandateRule;
using Longstone.Domain.Common;
using Longstone.Domain.Compliance;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Compliance;

public class AddMandateRuleHandlerTests
{
    private readonly IMandateRuleRepository _mandateRuleRepository = Substitute.For<IMandateRuleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly AddMandateRuleCommandHandler _handler;

    public AddMandateRuleHandlerTests()
    {
        _handler = new AddMandateRuleCommandHandler(_mandateRuleRepository, _unitOfWork, _timeProvider);
    }

    private static AddMandateRuleCommand ValidCommand() =>
        new(
            FundId: Guid.NewGuid(),
            RuleType: MandateRuleType.MaxSingleStockWeight,
            Parameters: """{"maxWeight": 0.10}""",
            Severity: RuleSeverity.Hard,
            EffectiveFrom: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo: null);

    [Fact]
    public async Task Handle_WithValidData_CreatesRuleAndReturnsId()
    {
        var command = ValidCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        await _mandateRuleRepository.Received(1).AddAsync(
            Arg.Is<MandateRule>(r =>
                r.FundId == command.FundId &&
                r.RuleType == command.RuleType &&
                r.Parameters == command.Parameters &&
                r.Severity == command.Severity &&
                r.IsActive),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEffectiveTo_CreatesRuleWithDateRange()
    {
        var command = ValidCommand() with
        {
            EffectiveTo = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        await _mandateRuleRepository.Received(1).AddAsync(
            Arg.Is<MandateRule>(r =>
                r.EffectiveFrom == command.EffectiveFrom &&
                r.EffectiveTo == command.EffectiveTo),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validate_WithValidCommand_PassesValidation()
    {
        var validator = new AddMandateRuleValidator();
        var command = ValidCommand();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyFundId_FailsValidation()
    {
        var validator = new AddMandateRuleValidator();
        var command = ValidCommand() with { FundId = Guid.Empty };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FundId");
    }

    [Fact]
    public async Task Validate_WithEmptyParameters_FailsValidation()
    {
        var validator = new AddMandateRuleValidator();
        var command = ValidCommand() with { Parameters = "" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Parameters");
    }

    [Fact]
    public async Task Validate_WithInvalidRuleType_FailsValidation()
    {
        var validator = new AddMandateRuleValidator();
        var command = ValidCommand() with { RuleType = (MandateRuleType)999 };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RuleType");
    }

    [Fact]
    public async Task Validate_WithInvalidSeverity_FailsValidation()
    {
        var validator = new AddMandateRuleValidator();
        var command = ValidCommand() with { Severity = (RuleSeverity)999 };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Severity");
    }

    [Fact]
    public async Task Validate_WithEffectiveToBeforeEffectiveFrom_FailsValidation()
    {
        var validator = new AddMandateRuleValidator();
        var command = ValidCommand() with
        {
            EffectiveFrom = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EffectiveTo");
    }

    [Fact]
    public async Task Validate_WithParametersExceedingMaxLength_FailsValidation()
    {
        var validator = new AddMandateRuleValidator();
        var command = ValidCommand() with { Parameters = new string('x', 5001) };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Parameters");
    }
}
