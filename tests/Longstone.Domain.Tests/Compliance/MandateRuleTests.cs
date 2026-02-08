using FluentAssertions;
using Longstone.Domain.Compliance;
using Microsoft.Extensions.Time.Testing;

namespace Longstone.Domain.Tests.Compliance;

public class MandateRuleTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly Guid _fundId = Guid.NewGuid();

    private MandateRule CreateValidRule(
        Guid? fundId = null,
        MandateRuleType ruleType = MandateRuleType.MaxSingleStockWeight,
        string parameters = """{"maxWeight": 0.10}""",
        RuleSeverity severity = RuleSeverity.Hard,
        DateTime? effectiveFrom = null,
        DateTime? effectiveTo = null)
    {
        return MandateRule.Create(
            fundId ?? _fundId,
            ruleType,
            parameters,
            severity,
            effectiveFrom ?? new DateTime(2025, 1, 1),
            effectiveTo,
            _timeProvider);
    }

    [Fact]
    public void Create_WithValidInputs_SetsAllProperties()
    {
        var effectiveFrom = new DateTime(2025, 1, 1);
        var effectiveTo = new DateTime(2026, 12, 31);

        var rule = CreateValidRule(effectiveFrom: effectiveFrom, effectiveTo: effectiveTo);

        rule.Id.Should().NotBe(Guid.Empty);
        rule.FundId.Should().Be(_fundId);
        rule.RuleType.Should().Be(MandateRuleType.MaxSingleStockWeight);
        rule.Parameters.Should().Be("""{"maxWeight": 0.10}""");
        rule.Severity.Should().Be(RuleSeverity.Hard);
        rule.IsActive.Should().BeTrue();
        rule.EffectiveFrom.Should().Be(effectiveFrom);
        rule.EffectiveTo.Should().Be(effectiveTo);
        rule.CreatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        rule.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var rule1 = CreateValidRule(ruleType: MandateRuleType.MaxSingleStockWeight);
        var rule2 = CreateValidRule(ruleType: MandateRuleType.MaxSectorExposure);

        rule1.Id.Should().NotBe(rule2.Id);
    }

    [Fact]
    public void Create_DefaultsToActive()
    {
        var rule = CreateValidRule();

        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNullEffectiveTo_SetsNull()
    {
        var rule = CreateValidRule(effectiveTo: null);

        rule.EffectiveTo.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyFundId_Throws()
    {
        var act = () => CreateValidRule(fundId: Guid.Empty);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("fundId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidParameters_Throws(string? parameters)
    {
        var act = () => CreateValidRule(parameters: parameters!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("parameters");
    }

    [Fact]
    public void Create_WithNullTimeProvider_Throws()
    {
        var act = () => MandateRule.Create(_fundId, MandateRuleType.MaxSingleStockWeight, """{"maxWeight": 0.10}""", RuleSeverity.Hard, new DateTime(2025, 1, 1), null, null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Create_WithEffectiveToBeforeEffectiveFrom_Throws()
    {
        var act = () => CreateValidRule(
            effectiveFrom: new DateTime(2026, 1, 1),
            effectiveTo: new DateTime(2025, 1, 1));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*before*");
    }

    // Activation / deactivation tests

    [Fact]
    public void Deactivate_WhenActive_SetsInactiveAndUpdatesTimestamp()
    {
        var rule = CreateValidRule();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        rule.Deactivate(_timeProvider);

        rule.IsActive.Should().BeFalse();
        rule.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        rule.UpdatedAt.Should().BeAfter(rule.CreatedAt);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_RemainsInactiveAndUpdatesTimestamp()
    {
        var rule = CreateValidRule();
        rule.Deactivate(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        rule.Deactivate(_timeProvider);

        rule.IsActive.Should().BeFalse();
        rule.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Activate_WhenInactive_SetsActiveAndUpdatesTimestamp()
    {
        var rule = CreateValidRule();
        rule.Deactivate(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        rule.Activate(_timeProvider);

        rule.IsActive.Should().BeTrue();
        rule.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_RemainsActiveAndUpdatesTimestamp()
    {
        var rule = CreateValidRule();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        rule.Activate(_timeProvider);

        rule.IsActive.Should().BeTrue();
        rule.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    // Null TimeProvider on state transitions

    [Fact]
    public void Deactivate_WithNullTimeProvider_Throws()
    {
        var rule = CreateValidRule();

        var act = () => rule.Deactivate(null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Activate_WithNullTimeProvider_Throws()
    {
        var rule = CreateValidRule();
        rule.Deactivate(_timeProvider);

        var act = () => rule.Activate(null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    // Update effective dates

    [Fact]
    public void UpdateEffectiveDates_WithValidDates_UpdatesDatesAndTimestamp()
    {
        var rule = CreateValidRule();
        var newFrom = new DateTime(2025, 6, 1);
        var newTo = new DateTime(2027, 12, 31);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        rule.UpdateEffectiveDates(newFrom, newTo, _timeProvider);

        rule.EffectiveFrom.Should().Be(newFrom);
        rule.EffectiveTo.Should().Be(newTo);
        rule.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void UpdateEffectiveDates_WithNullEffectiveTo_ClearsEndDate()
    {
        var rule = CreateValidRule(effectiveTo: new DateTime(2026, 12, 31));

        rule.UpdateEffectiveDates(new DateTime(2025, 1, 1), null, _timeProvider);

        rule.EffectiveTo.Should().BeNull();
    }

    [Fact]
    public void UpdateEffectiveDates_WithEffectiveToBeforeFrom_Throws()
    {
        var rule = CreateValidRule();

        var act = () => rule.UpdateEffectiveDates(
            new DateTime(2026, 1, 1),
            new DateTime(2025, 1, 1),
            _timeProvider);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*before*");
    }

    [Fact]
    public void UpdateEffectiveDates_WithNullTimeProvider_Throws()
    {
        var rule = CreateValidRule();

        var act = () => rule.UpdateEffectiveDates(new DateTime(2025, 1, 1), null, null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    // Enum validation tests

    [Fact]
    public void Create_WithInvalidRuleType_Throws()
    {
        var act = () => CreateValidRule(ruleType: (MandateRuleType)999);

        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("ruleType");
    }

    [Fact]
    public void Create_WithInvalidSeverity_Throws()
    {
        var act = () => CreateValidRule(severity: (RuleSeverity)999);

        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("severity");
    }

    // Severity tests

    [Theory]
    [InlineData(RuleSeverity.Hard)]
    [InlineData(RuleSeverity.Soft)]
    public void Create_WithEachSeverity_SetsCorrectly(RuleSeverity severity)
    {
        var rule = CreateValidRule(severity: severity);

        rule.Severity.Should().Be(severity);
    }

    [Fact]
    public void RuleSeverity_HasExactlyTwoValues()
    {
        var values = Enum.GetValues<RuleSeverity>();

        values.Should().HaveCount(2);
    }

    // MandateRuleType enum tests

    [Fact]
    public void MandateRuleType_HasExpectedValues()
    {
        var values = Enum.GetValues<MandateRuleType>();

        values.Should().Contain(MandateRuleType.MaxSingleStockWeight);
        values.Should().Contain(MandateRuleType.MaxSectorExposure);
        values.Should().Contain(MandateRuleType.MaxCountryExposure);
        values.Should().Contain(MandateRuleType.MinCashHolding);
        values.Should().Contain(MandateRuleType.BannedInstrument);
        values.Should().Contain(MandateRuleType.AssetClassLimit);
        values.Should().Contain(MandateRuleType.MarketCapFloor);
        values.Should().Contain(MandateRuleType.MaxHoldings);
        values.Should().Contain(MandateRuleType.CurrencyExposureLimit);
        values.Should().Contain(MandateRuleType.TrackingErrorLimit);
        values.Should().HaveCount(10);
    }

    // Rule type parametrized test

    [Theory]
    [InlineData(MandateRuleType.MaxSingleStockWeight)]
    [InlineData(MandateRuleType.MaxSectorExposure)]
    [InlineData(MandateRuleType.MaxCountryExposure)]
    [InlineData(MandateRuleType.MinCashHolding)]
    [InlineData(MandateRuleType.BannedInstrument)]
    [InlineData(MandateRuleType.AssetClassLimit)]
    [InlineData(MandateRuleType.MarketCapFloor)]
    [InlineData(MandateRuleType.MaxHoldings)]
    [InlineData(MandateRuleType.CurrencyExposureLimit)]
    [InlineData(MandateRuleType.TrackingErrorLimit)]
    public void Create_WithEachRuleType_SetsCorrectly(MandateRuleType ruleType)
    {
        var rule = CreateValidRule(ruleType: ruleType);

        rule.RuleType.Should().Be(ruleType);
    }
}
