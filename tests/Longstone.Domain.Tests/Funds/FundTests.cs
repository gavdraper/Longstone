using FluentAssertions;
using Longstone.Domain.Funds;
using Microsoft.Extensions.Time.Testing;

namespace Longstone.Domain.Tests.Funds;

public class FundTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));

    private Fund CreateValidFund(
        string name = "UK Growth Fund",
        string lei = "549300EXAMPLE000001X",
        string isin = "GB00B3X7QG63",
        FundType fundType = FundType.OEIC,
        string baseCurrency = "GBP",
        string? benchmarkIndex = "FTSE 100",
        DateTime? inceptionDate = null)
    {
        return Fund.Create(
            name,
            lei,
            isin,
            fundType,
            baseCurrency,
            benchmarkIndex,
            inceptionDate ?? new DateTime(2025, 1, 1),
            _timeProvider);
    }

    [Fact]
    public void Create_WithValidInputs_SetsAllProperties()
    {
        var inceptionDate = new DateTime(2025, 1, 1);

        var fund = CreateValidFund(inceptionDate: inceptionDate);

        fund.Id.Should().NotBe(Guid.Empty);
        fund.Name.Should().Be("UK Growth Fund");
        fund.Lei.Should().Be("549300EXAMPLE000001X");
        fund.Isin.Should().Be("GB00B3X7QG63");
        fund.FundType.Should().Be(FundType.OEIC);
        fund.BaseCurrency.Should().Be("GBP");
        fund.BenchmarkIndex.Should().Be("FTSE 100");
        fund.InceptionDate.Should().Be(inceptionDate);
        fund.Status.Should().Be(FundStatus.Active);
        fund.CreatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var fund1 = CreateValidFund(name: "Fund A", isin: "GB00B3X7QG63");
        var fund2 = CreateValidFund(name: "Fund B", isin: "GB00B7W6PR65");

        fund1.Id.Should().NotBe(fund2.Id);
    }

    [Fact]
    public void Create_DefaultsToActiveStatus()
    {
        var fund = CreateValidFund();

        fund.Status.Should().Be(FundStatus.Active);
    }

    [Fact]
    public void Create_WithNullBenchmarkIndex_SetsNull()
    {
        var fund = CreateValidFund(benchmarkIndex: null);

        fund.BenchmarkIndex.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_Throws(string? name)
    {
        var act = () => CreateValidFund(name: name!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidLei_Throws(string? lei)
    {
        var act = () => CreateValidFund(lei: lei!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("lei");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidIsin_Throws(string? isin)
    {
        var act = () => CreateValidFund(isin: isin!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("isin");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidBaseCurrency_Throws(string? baseCurrency)
    {
        var act = () => CreateValidFund(baseCurrency: baseCurrency!);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("baseCurrency");
    }

    [Fact]
    public void Create_WithNullTimeProvider_Throws()
    {
        var act = () => Fund.Create("UK Growth Fund", "549300EXAMPLE000001X", "GB00B3X7QG63", FundType.OEIC, "GBP", "FTSE 100", new DateTime(2025, 1, 1), null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    // Status transition tests

    [Fact]
    public void Suspend_FromActive_SetsSuspendedAndUpdatesTimestamp()
    {
        var fund = CreateValidFund();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.Suspend(_timeProvider);

        fund.Status.Should().Be(FundStatus.Suspended);
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        fund.UpdatedAt.Should().BeAfter(fund.CreatedAt);
    }

    [Fact]
    public void Suspend_FromClosed_Throws()
    {
        var fund = CreateValidFund();
        fund.Close(_timeProvider);

        var act = () => fund.Suspend(_timeProvider);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot suspend a closed fund.");
    }

    [Fact]
    public void Close_FromActive_SetsClosedAndUpdatesTimestamp()
    {
        var fund = CreateValidFund();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.Close(_timeProvider);

        fund.Status.Should().Be(FundStatus.Closed);
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        fund.UpdatedAt.Should().BeAfter(fund.CreatedAt);
    }

    [Fact]
    public void Close_FromSuspended_SetsClosed()
    {
        var fund = CreateValidFund();
        fund.Suspend(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.Close(_timeProvider);

        fund.Status.Should().Be(FundStatus.Closed);
    }

    [Fact]
    public void Reactivate_FromSuspended_SetsActive()
    {
        var fund = CreateValidFund();
        fund.Suspend(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.Reactivate(_timeProvider);

        fund.Status.Should().Be(FundStatus.Active);
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Suspend_FromSuspended_RemainsSuspendedAndUpdatesTimestamp()
    {
        var fund = CreateValidFund();
        fund.Suspend(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.Suspend(_timeProvider);

        fund.Status.Should().Be(FundStatus.Suspended);
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Reactivate_FromActive_RemainsActiveAndUpdatesTimestamp()
    {
        var fund = CreateValidFund();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.Reactivate(_timeProvider);

        fund.Status.Should().Be(FundStatus.Active);
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Reactivate_FromClosed_Throws()
    {
        var fund = CreateValidFund();
        fund.Close(_timeProvider);

        var act = () => fund.Reactivate(_timeProvider);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot reactivate a closed fund.");
    }

    // Null TimeProvider on state transitions

    [Fact]
    public void Suspend_WithNullTimeProvider_Throws()
    {
        var fund = CreateValidFund();

        var act = () => fund.Suspend(null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Close_WithNullTimeProvider_Throws()
    {
        var fund = CreateValidFund();

        var act = () => fund.Close(null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void Reactivate_WithNullTimeProvider_Throws()
    {
        var fund = CreateValidFund();
        fund.Suspend(_timeProvider);

        var act = () => fund.Reactivate(null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    // Fund manager assignment tests

    [Fact]
    public void AssignManager_WithValidUserId_AddsToAssignedManagers()
    {
        var fund = CreateValidFund();
        var userId = Guid.NewGuid();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.AssignManager(userId, _timeProvider);

        fund.AssignedManagers.Should().HaveCount(1);
        fund.AssignedManagers.First().UserId.Should().Be(userId);
        fund.AssignedManagers.First().FundId.Should().Be(fund.Id);
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void AssignManager_SameUserTwice_Throws()
    {
        var fund = CreateValidFund();
        var userId = Guid.NewGuid();
        fund.AssignManager(userId, _timeProvider);

        var act = () => fund.AssignManager(userId, _timeProvider);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User is already assigned as a manager of this fund.");
    }

    [Fact]
    public void AssignManager_WithEmptyUserId_Throws()
    {
        var fund = CreateValidFund();

        var act = () => fund.AssignManager(Guid.Empty, _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void AssignManager_WithNullTimeProvider_Throws()
    {
        var fund = CreateValidFund();

        var act = () => fund.AssignManager(Guid.NewGuid(), null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void RemoveManager_WithAssignedUser_RemovesFromManagers()
    {
        var fund = CreateValidFund();
        var userId = Guid.NewGuid();
        fund.AssignManager(userId, _timeProvider);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.RemoveManager(userId, _timeProvider);

        fund.AssignedManagers.Should().BeEmpty();
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void RemoveManager_NotAssigned_Throws()
    {
        var fund = CreateValidFund();
        var userId = Guid.NewGuid();

        var act = () => fund.RemoveManager(userId, _timeProvider);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User is not assigned as a manager of this fund.");
    }

    [Fact]
    public void RemoveManager_WithNullTimeProvider_Throws()
    {
        var fund = CreateValidFund();
        var userId = Guid.NewGuid();
        fund.AssignManager(userId, _timeProvider);

        var act = () => fund.RemoveManager(userId, null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    // UpdateDetails tests

    [Fact]
    public void UpdateDetails_WithValidInputs_UpdatesAllProperties()
    {
        var fund = CreateValidFund();
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        fund.UpdateDetails("New Name", "549300UPDATED000001X", "GB00BUPDATE1",
            FundType.UnitTrust, "USD", "S&P 500",
            new DateTime(2025, 6, 1), _timeProvider);

        fund.Name.Should().Be("New Name");
        fund.Lei.Should().Be("549300UPDATED000001X");
        fund.Isin.Should().Be("GB00BUPDATE1");
        fund.FundType.Should().Be(FundType.UnitTrust);
        fund.BaseCurrency.Should().Be("USD");
        fund.BenchmarkIndex.Should().Be("S&P 500");
        fund.InceptionDate.Should().Be(new DateTime(2025, 6, 1));
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        fund.UpdatedAt.Should().BeAfter(fund.CreatedAt);
    }

    [Fact]
    public void UpdateDetails_WithNullBenchmark_SetsNull()
    {
        var fund = CreateValidFund();

        fund.UpdateDetails("Name", "549300EXAMPLE000001X", "GB00B3X7QG63",
            FundType.OEIC, "GBP", null, new DateTime(2025, 1, 1), _timeProvider);

        fund.BenchmarkIndex.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_WithInvalidName_Throws(string? name)
    {
        var fund = CreateValidFund();

        var act = () => fund.UpdateDetails(name!, "549300EXAMPLE000001X", "GB00B3X7QG63",
            FundType.OEIC, "GBP", null, new DateTime(2025, 1, 1), _timeProvider);

        act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
    }

    [Fact]
    public void UpdateDetails_WithNullTimeProvider_Throws()
    {
        var fund = CreateValidFund();

        var act = () => fund.UpdateDetails("Name", "549300EXAMPLE000001X", "GB00B3X7QG63",
            FundType.OEIC, "GBP", null, new DateTime(2025, 1, 1), null!);

        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("timeProvider");
    }

    [Fact]
    public void UpdateDetails_WithInvalidFundType_Throws()
    {
        var fund = CreateValidFund();

        var act = () => fund.UpdateDetails("Name", "549300EXAMPLE000001X", "GB00B3X7QG63",
            (FundType)999, "GBP", null, new DateTime(2025, 1, 1), _timeProvider);

        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("fundType");
    }

    // Enum validation tests

    [Fact]
    public void Create_WithInvalidFundType_Throws()
    {
        var act = () => CreateValidFund(fundType: (FundType)999);

        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("fundType");
    }

    // FundType enum tests

    [Theory]
    [InlineData(FundType.OEIC)]
    [InlineData(FundType.UnitTrust)]
    [InlineData(FundType.InvestmentTrust)]
    [InlineData(FundType.SegregatedMandate)]
    public void Create_WithEachFundType_SetsCorrectly(FundType fundType)
    {
        var fund = CreateValidFund(fundType: fundType);

        fund.FundType.Should().Be(fundType);
    }

    [Fact]
    public void FundType_HasExactlyFourValues()
    {
        var values = Enum.GetValues<FundType>();

        values.Should().HaveCount(4);
    }

    [Fact]
    public void FundStatus_HasExactlyThreeValues()
    {
        var values = Enum.GetValues<FundStatus>();

        values.Should().HaveCount(3);
    }

    // AssignedManagers collection is initially empty

    [Fact]
    public void Create_AssignedManagers_IsEmptyByDefault()
    {
        var fund = CreateValidFund();

        fund.AssignedManagers.Should().BeEmpty();
    }
}
