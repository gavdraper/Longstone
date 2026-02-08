using Longstone.Domain.Auth;
using Longstone.Domain.Compliance;
using Longstone.Domain.Funds;

namespace Longstone.Infrastructure.Persistence.Seed;

public static class FundSeedData
{
    public static (IReadOnlyList<Fund> Funds, IReadOnlyList<MandateRule> Rules) CreateSeededFunds(
        IReadOnlyList<User> users,
        TimeProvider timeProvider)
    {
        var funds = new List<Fund>();
        var rules = new List<MandateRule>();

        var fundManager = users.First(u => u.Role == Role.FundManager);

        // 1. UK Equity Income — OEIC, Active
        var ukEquityIncome = Fund.Create(
            "Longstone UK Equity Income",
            "549300EXAMPLE0UKEQ01",
            "GB00LSUKEQ01",
            FundType.OEIC,
            "GBP",
            "FTSE All-Share",
            new DateTime(2018, 4, 6, 0, 0, 0, DateTimeKind.Utc),
            timeProvider);
        ukEquityIncome.AssignManager(fundManager.Id, timeProvider);
        funds.Add(ukEquityIncome);

        rules.Add(MandateRule.Create(ukEquityIncome.Id, MandateRuleType.MaxSingleStockWeight,
            """{"maxWeight": 0.10}""", RuleSeverity.Hard,
            new DateTime(2018, 4, 6, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(ukEquityIncome.Id, MandateRuleType.MaxSectorExposure,
            """{"maxWeight": 0.30}""", RuleSeverity.Hard,
            new DateTime(2018, 4, 6, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(ukEquityIncome.Id, MandateRuleType.MinCashHolding,
            """{"minWeight": 0.02}""", RuleSeverity.Soft,
            new DateTime(2018, 4, 6, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));

        // 2. Global Growth — OEIC, Active
        var globalGrowth = Fund.Create(
            "Longstone Global Growth",
            "549300EXAMPLE0GLGR02",
            "GB00LSGLGR02",
            FundType.OEIC,
            "GBP",
            "MSCI World",
            new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            timeProvider);
        globalGrowth.AssignManager(fundManager.Id, timeProvider);
        funds.Add(globalGrowth);

        rules.Add(MandateRule.Create(globalGrowth.Id, MandateRuleType.MaxSingleStockWeight,
            """{"maxWeight": 0.08}""", RuleSeverity.Hard,
            new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(globalGrowth.Id, MandateRuleType.MaxCountryExposure,
            """{"maxWeight": 0.40}""", RuleSeverity.Hard,
            new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(globalGrowth.Id, MandateRuleType.CurrencyExposureLimit,
            """{"maxWeight": 0.50, "currency": "USD"}""", RuleSeverity.Soft,
            new DateTime(2019, 1, 2, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));

        // 3. UK Gilt — Unit Trust, Active
        var ukGilt = Fund.Create(
            "Longstone UK Gilt",
            "549300EXAMPLE0UKGL03",
            "GB00LSUKGL03",
            FundType.UnitTrust,
            "GBP",
            "FTSE Actuaries UK Gilts All Stocks",
            new DateTime(2020, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            timeProvider);
        ukGilt.AssignManager(fundManager.Id, timeProvider);
        funds.Add(ukGilt);

        rules.Add(MandateRule.Create(ukGilt.Id, MandateRuleType.AssetClassLimit,
            """{"assetClass": "FixedIncome", "minWeight": 0.80}""", RuleSeverity.Hard,
            new DateTime(2020, 7, 1, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(ukGilt.Id, MandateRuleType.MinCashHolding,
            """{"minWeight": 0.05}""", RuleSeverity.Hard,
            new DateTime(2020, 7, 1, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));

        // 4. Multi-Asset Balanced — OEIC, Active
        var multiAsset = Fund.Create(
            "Longstone Multi-Asset Balanced",
            "549300EXAMPLE0MABA04",
            "GB00LSMABA04",
            FundType.OEIC,
            "GBP",
            "ARC Sterling Balanced",
            new DateTime(2021, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            timeProvider);
        multiAsset.AssignManager(fundManager.Id, timeProvider);
        funds.Add(multiAsset);

        rules.Add(MandateRule.Create(multiAsset.Id, MandateRuleType.MaxSingleStockWeight,
            """{"maxWeight": 0.05}""", RuleSeverity.Hard,
            new DateTime(2021, 3, 15, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(multiAsset.Id, MandateRuleType.AssetClassLimit,
            """{"assetClass": "Equity", "maxWeight": 0.60}""", RuleSeverity.Hard,
            new DateTime(2021, 3, 15, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(multiAsset.Id, MandateRuleType.CurrencyExposureLimit,
            """{"maxWeight": 0.30, "currency": "USD"}""", RuleSeverity.Soft,
            new DateTime(2021, 3, 15, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));

        // 5. UK Small Cap — Investment Trust, Suspended
        var ukSmallCap = Fund.Create(
            "Longstone UK Small Cap",
            "549300EXAMPLE0UKSC05",
            "GB00LSUKSC05",
            FundType.InvestmentTrust,
            "GBP",
            "FTSE Small Cap",
            new DateTime(2017, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            timeProvider);
        ukSmallCap.AssignManager(fundManager.Id, timeProvider);
        ukSmallCap.Suspend(timeProvider);
        funds.Add(ukSmallCap);

        rules.Add(MandateRule.Create(ukSmallCap.Id, MandateRuleType.MarketCapFloor,
            """{"maxMarketCap": 2000000000}""", RuleSeverity.Hard,
            new DateTime(2017, 9, 1, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(ukSmallCap.Id, MandateRuleType.MaxHoldings,
            """{"maxHoldings": 80}""", RuleSeverity.Soft,
            new DateTime(2017, 9, 1, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));

        // 6. Segregated Mandate — Pension, Active
        var segregatedPension = Fund.Create(
            "Longstone Pension Segregated",
            "549300EXAMPLE0PNSM06",
            "GB00LSPNSM06",
            FundType.SegregatedMandate,
            "GBP",
            null,
            new DateTime(2022, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            timeProvider);
        segregatedPension.AssignManager(fundManager.Id, timeProvider);
        funds.Add(segregatedPension);

        rules.Add(MandateRule.Create(segregatedPension.Id, MandateRuleType.MaxSingleStockWeight,
            """{"maxWeight": 0.05}""", RuleSeverity.Hard,
            new DateTime(2022, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(segregatedPension.Id, MandateRuleType.BannedInstrument,
            """{"reason": "Ethical exclusion", "sectors": ["Tobacco", "Gambling", "Weapons"]}""", RuleSeverity.Hard,
            new DateTime(2022, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));
        rules.Add(MandateRule.Create(segregatedPension.Id, MandateRuleType.TrackingErrorLimit,
            """{"maxTrackingError": 0.03}""", RuleSeverity.Soft,
            new DateTime(2022, 1, 10, 0, 0, 0, DateTimeKind.Utc), null, timeProvider));

        return (funds, rules);
    }
}
