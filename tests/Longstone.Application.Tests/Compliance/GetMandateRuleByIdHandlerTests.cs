using FluentAssertions;
using Longstone.Application.Compliance.Queries;
using Longstone.Application.Compliance.Queries.GetMandateRuleById;
using Longstone.Domain.Compliance;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Compliance;

public class GetMandateRuleByIdHandlerTests
{
    private readonly IMandateRuleRepository _mandateRuleRepository = Substitute.For<IMandateRuleRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly GetMandateRuleByIdHandler _handler;

    public GetMandateRuleByIdHandlerTests()
    {
        _handler = new GetMandateRuleByIdHandler(_mandateRuleRepository);
    }

    [Fact]
    public async Task Handle_WithExistingRule_ReturnsDto()
    {
        var rule = MandateRule.Create(
            fundId: Guid.NewGuid(),
            ruleType: MandateRuleType.MaxSingleStockWeight,
            parameters: """{"maxWeight": 0.10}""",
            severity: RuleSeverity.Hard,
            effectiveFrom: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            effectiveTo: null,
            timeProvider: _timeProvider);

        _mandateRuleRepository.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>())
            .Returns(rule);

        var query = new GetMandateRuleByIdQuery(rule.Id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(rule.Id);
        result.RuleType.Should().Be(MandateRuleType.MaxSingleStockWeight);
    }

    [Fact]
    public async Task Handle_WithNonExistentRule_ReturnsNull()
    {
        var ruleId = Guid.NewGuid();
        _mandateRuleRepository.GetByIdAsync(ruleId, Arg.Any<CancellationToken>())
            .Returns((MandateRule?)null);

        var query = new GetMandateRuleByIdQuery(ruleId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
