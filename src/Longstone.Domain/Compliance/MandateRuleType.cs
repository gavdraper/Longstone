namespace Longstone.Domain.Compliance;

public enum MandateRuleType
{
    MaxSingleStockWeight,
    MaxSectorExposure,
    MaxCountryExposure,
    MinCashHolding,
    BannedInstrument,
    AssetClassLimit,
    MarketCapFloor,
    MaxHoldings,
    CurrencyExposureLimit,
    TrackingErrorLimit
}
