using FluentAssertions;
using Longstone.Application.Common.Exceptions;
using Longstone.Application.Compliance.Commands.ToggleMandateRule;
using Longstone.Domain.Common;
using Longstone.Domain.Compliance;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Compliance;

public class ToggleMandateRuleHandlerTests
{
    private readonly IMandateRuleRepository _mandateRuleRepository = Substitute.For<IMandateRuleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly ToggleMandateRuleCommandHandler _handler;

    public ToggleMandateRuleHandlerTests()
    {
        _handler = new ToggleMandateRuleCommandHandler(_mandateRuleRepository, _unitOfWork, _timeProvider);
    }

    private MandateRule CreateActiveRule(Guid? id = null)
    {
        var rule = MandateRule.Create(
            fundId: Guid.NewGuid(),
            ruleType: MandateRuleType.MaxSectorExposure,
            parameters: """{"maxExposure": 0.25}""",
            severity: RuleSeverity.Hard,
            effectiveFrom: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            effectiveTo: null,
            timeProvider: _timeProvider);

        if (id.HasValue)
        {
            typeof(MandateRule).GetProperty(nameof(MandateRule.Id))!
                .SetValue(rule, id.Value);
        }

        return rule;
    }

    private MandateRule CreateInactiveRule(Guid? id = null)
    {
        var rule = CreateActiveRule(id);
        rule.Deactivate(_timeProvider);
        return rule;
    }

    [Fact]
    public async Task Handle_DeactivateActiveRule_DeactivatesAndSaves()
    {
        var ruleId = Guid.NewGuid();
        var rule = CreateActiveRule(ruleId);
        _mandateRuleRepository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns(rule);

        var command = new ToggleMandateRuleCommand(ruleId, IsActive: false);
        await _handler.Handle(command, CancellationToken.None);

        rule.IsActive.Should().BeFalse();
        _mandateRuleRepository.Received(1).Update(rule);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActivateInactiveRule_ActivatesAndSaves()
    {
        var ruleId = Guid.NewGuid();
        var rule = CreateInactiveRule(ruleId);
        _mandateRuleRepository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns(rule);

        var command = new ToggleMandateRuleCommand(ruleId, IsActive: true);
        await _handler.Handle(command, CancellationToken.None);

        rule.IsActive.Should().BeTrue();
        _mandateRuleRepository.Received(1).Update(rule);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentRule_ThrowsNotFoundException()
    {
        var ruleId = Guid.NewGuid();
        _mandateRuleRepository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns((MandateRule?)null);

        var command = new ToggleMandateRuleCommand(ruleId, IsActive: false);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Validate_WithValidCommand_PassesValidation()
    {
        var validator = new ToggleMandateRuleValidator();
        var command = new ToggleMandateRuleCommand(Guid.NewGuid(), IsActive: false);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyId_FailsValidation()
    {
        var validator = new ToggleMandateRuleValidator();
        var command = new ToggleMandateRuleCommand(Guid.Empty, IsActive: false);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}
