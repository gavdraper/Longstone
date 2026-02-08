using FluentAssertions;
using Longstone.Application.Compliance.Queries;
using Longstone.Application.Compliance.Queries.GetMandateRulesByFund;
using Longstone.Domain.Compliance;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Compliance;

public class GetMandateRulesByFundHandlerTests
{
    private readonly IMandateRuleRepository _mandateRuleRepository = Substitute.For<IMandateRuleRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly GetMandateRulesByFundHandler _handler;

    public GetMandateRulesByFundHandlerTests()
    {
        _handler = new GetMandateRulesByFundHandler(_mandateRuleRepository);
    }

    private MandateRule CreateRule(Guid fundId, MandateRuleType ruleType, bool isActive = true)
    {
        var rule = MandateRule.Create(
            fundId: fundId,
            ruleType: ruleType,
            parameters: """{"maxWeight": 0.10}""",
            severity: RuleSeverity.Hard,
            effectiveFrom: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            effectiveTo: null,
            timeProvider: _timeProvider);

        if (!isActive)
        {
            rule.Deactivate(_timeProvider);
        }

        return rule;
    }

    [Fact]
    public async Task Handle_ReturnsAllRulesForFund()
    {
        var fundId = Guid.NewGuid();
        var rules = new List<MandateRule>
        {
            CreateRule(fundId, MandateRuleType.MaxSingleStockWeight),
            CreateRule(fundId, MandateRuleType.MaxSectorExposure)
        };

        _mandateRuleRepository.GetByFundAsync(fundId, Arg.Any<CancellationToken>())
            .Returns(rules.AsReadOnly());

        var query = new GetMandateRulesByFundQuery(fundId, ActiveOnly: false);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithActiveOnly_ReturnsOnlyActiveRules()
    {
        var fundId = Guid.NewGuid();
        var activeRules = new List<MandateRule>
        {
            CreateRule(fundId, MandateRuleType.MaxSingleStockWeight)
        };

        _mandateRuleRepository.GetActiveByFundAsync(fundId, Arg.Any<CancellationToken>())
            .Returns(activeRules.AsReadOnly());

        var query = new GetMandateRulesByFundQuery(fundId, ActiveOnly: true);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        await _mandateRuleRepository.Received(1).GetActiveByFundAsync(fundId, Arg.Any<CancellationToken>());
        await _mandateRuleRepository.DidNotReceive().GetByFundAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoRules_ReturnsEmptyList()
    {
        var fundId = Guid.NewGuid();
        _mandateRuleRepository.GetByFundAsync(fundId, Arg.Any<CancellationToken>())
            .Returns(new List<MandateRule>().AsReadOnly());

        var query = new GetMandateRulesByFundQuery(fundId, ActiveOnly: false);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsDtoCorrectly()
    {
        var fundId = Guid.NewGuid();
        var rule = CreateRule(fundId, MandateRuleType.MaxSingleStockWeight);
        _mandateRuleRepository.GetByFundAsync(fundId, Arg.Any<CancellationToken>())
            .Returns(new List<MandateRule> { rule }.AsReadOnly());

        var query = new GetMandateRulesByFundQuery(fundId, ActiveOnly: false);
        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result[0];
        dto.Id.Should().Be(rule.Id);
        dto.FundId.Should().Be(fundId);
        dto.RuleType.Should().Be(MandateRuleType.MaxSingleStockWeight);
        dto.Parameters.Should().Be("""{"maxWeight": 0.10}""");
        dto.Severity.Should().Be(RuleSeverity.Hard);
        dto.IsActive.Should().BeTrue();
        dto.EffectiveFrom.Should().Be(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        dto.EffectiveTo.Should().BeNull();
    }
}
