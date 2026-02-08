using FluentAssertions;
using Longstone.Application.Common.Exceptions;
using Longstone.Application.Compliance.Commands.UpdateMandateRule;
using Longstone.Domain.Common;
using Longstone.Domain.Compliance;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Compliance;

public class UpdateMandateRuleHandlerTests
{
    private readonly IMandateRuleRepository _mandateRuleRepository = Substitute.For<IMandateRuleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly UpdateMandateRuleCommandHandler _handler;

    public UpdateMandateRuleHandlerTests()
    {
        _handler = new UpdateMandateRuleCommandHandler(_mandateRuleRepository, _unitOfWork, _timeProvider);
    }

    private static UpdateMandateRuleCommand ValidCommand(Guid? ruleId = null) =>
        new(
            Id: ruleId ?? Guid.NewGuid(),
            Parameters: """{"maxWeight": 0.15}""",
            Severity: RuleSeverity.Soft,
            EffectiveFrom: new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo: new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc));

    private MandateRule CreateExistingRule(Guid? id = null)
    {
        var rule = MandateRule.Create(
            fundId: Guid.NewGuid(),
            ruleType: MandateRuleType.MaxSingleStockWeight,
            parameters: """{"maxWeight": 0.10}""",
            severity: RuleSeverity.Hard,
            effectiveFrom: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            effectiveTo: null,
            timeProvider: _timeProvider);

        if (id.HasValue)
        {
            // Use reflection to set the ID for testing
            typeof(MandateRule).GetProperty(nameof(MandateRule.Id))!
                .SetValue(rule, id.Value);
        }

        return rule;
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesRuleAndSaves()
    {
        var ruleId = Guid.NewGuid();
        var existingRule = CreateExistingRule(ruleId);
        _mandateRuleRepository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns(existingRule);

        var command = ValidCommand(ruleId);
        await _handler.Handle(command, CancellationToken.None);

        existingRule.Parameters.Should().Be(command.Parameters);
        existingRule.Severity.Should().Be(command.Severity);
        existingRule.EffectiveFrom.Should().Be(command.EffectiveFrom);
        existingRule.EffectiveTo.Should().Be(command.EffectiveTo);
        _mandateRuleRepository.Received(1).Update(existingRule);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentRule_ThrowsNotFoundException()
    {
        var ruleId = Guid.NewGuid();
        _mandateRuleRepository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns((MandateRule?)null);

        var command = ValidCommand(ruleId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Validate_WithValidCommand_PassesValidation()
    {
        var validator = new UpdateMandateRuleValidator();
        var command = ValidCommand();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyId_FailsValidation()
    {
        var validator = new UpdateMandateRuleValidator();
        var command = ValidCommand() with { Id = Guid.Empty };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public async Task Validate_WithEmptyParameters_FailsValidation()
    {
        var validator = new UpdateMandateRuleValidator();
        var command = ValidCommand() with { Parameters = "" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Parameters");
    }

    [Fact]
    public async Task Validate_WithInvalidSeverity_FailsValidation()
    {
        var validator = new UpdateMandateRuleValidator();
        var command = ValidCommand() with { Severity = (RuleSeverity)999 };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Severity");
    }

    [Fact]
    public async Task Validate_WithEffectiveToBeforeEffectiveFrom_FailsValidation()
    {
        var validator = new UpdateMandateRuleValidator();
        var command = ValidCommand() with
        {
            EffectiveFrom = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EffectiveTo");
    }
}
