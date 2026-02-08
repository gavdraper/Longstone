using Longstone.Domain.Compliance;
using Longstone.Domain.Funds;
using MudBlazor;

namespace Longstone.Web.Components.Pages.Funds;

public static class FundDisplayHelpers
{
    public static Color GetStatusColor(FundStatus status) => status switch
    {
        FundStatus.Active => Color.Success,
        FundStatus.Suspended => Color.Warning,
        FundStatus.Closed => Color.Error,
        _ => Color.Default
    };

    public static string FormatFundType(FundType type) => type switch
    {
        FundType.OEIC => "OEIC",
        FundType.UnitTrust => "Unit Trust",
        FundType.InvestmentTrust => "Investment Trust",
        FundType.SegregatedMandate => "Segregated Mandate",
        _ => type.ToString()
    };

    public static string FormatRuleType(MandateRuleType type) => type switch
    {
        MandateRuleType.MaxSingleStockWeight => "Max Single Stock Weight",
        MandateRuleType.MaxSectorExposure => "Max Sector Exposure",
        MandateRuleType.MaxCountryExposure => "Max Country Exposure",
        MandateRuleType.MinCashHolding => "Min Cash Holding",
        MandateRuleType.BannedInstrument => "Banned Instrument",
        MandateRuleType.AssetClassLimit => "Asset Class Limit",
        MandateRuleType.MarketCapFloor => "Market Cap Floor",
        MandateRuleType.MaxHoldings => "Max Holdings",
        MandateRuleType.CurrencyExposureLimit => "Currency Exposure Limit",
        MandateRuleType.TrackingErrorLimit => "Tracking Error Limit",
        _ => type.ToString()
    };
}
