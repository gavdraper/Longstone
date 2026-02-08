using Longstone.Domain.Instruments;

namespace Longstone.Infrastructure.Persistence.Seed;

public static class InstrumentSeedData
{
    public static IReadOnlyList<Instrument> CreateSeededInstruments(TimeProvider timeProvider)
    {
        var instruments = new List<Instrument>();

        // UK equities — Large caps (LSE)
        instruments.Add(CreateEquity("GB0007188757", "0718875", "SHEL", Exchange.LSE, "Shell plc", "GBP", "GB", "Energy", 180_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0002374006", "0237400", "DGE", Exchange.LSE, "Diageo plc", "GBP", "GB", "Consumer Staples", 65_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0009895292", "0989529", "AZN", Exchange.LSE, "AstraZeneca plc", "GBP", "GB", "Healthcare", 185_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00BH4HKS39", "BH4HKS3", "VOD", Exchange.LSE, "Vodafone Group plc", "GBP", "GB", "Telecommunications", 20_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0005405286", "0540528", "HSBA", Exchange.LSE, "HSBC Holdings plc", "GBP", "GB", "Financials", 130_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00B24CGK77", "B24CGK7", "RKT", Exchange.LSE, "Reckitt Benckiser Group plc", "GBP", "GB", "Consumer Staples", 38_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0007980591", "0798059", "BP.", Exchange.LSE, "BP plc", "GBP", "GB", "Energy", 85_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00B10RZP78", "B10RZP7", "ULVR", Exchange.LSE, "Unilever plc", "GBP", "GB", "Consumer Staples", 110_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0031348658", "3134865", "BARC", Exchange.LSE, "Barclays plc", "GBP", "GB", "Financials", 30_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00B03MLX29", "B03MLX2", "RR.", Exchange.LSE, "Rolls-Royce Holdings plc", "GBP", "GB", "Industrials", 35_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00BP6MXD84", "BP6MXD8", "JET", Exchange.LSE, "JET2 plc", "GBP", "GB", "Consumer Discretionary", 4_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0002875804", "0287580", "BA.", Exchange.LSE, "BAE Systems plc", "GBP", "GB", "Industrials", 45_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0009252882", "0925288", "GSK", Exchange.LSE, "GSK plc", "GBP", "GB", "Healthcare", 68_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00BN7SWP63", "BN7SWP6", "LSEG", Exchange.LSE, "London Stock Exchange Group plc", "GBP", "GB", "Financials", 55_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00BPQY8M80", "BPQY8M8", "AAL", Exchange.LSE, "Anglo American plc", "GBP", "GB", "Materials", 28_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00B1XZS820", "B1XZS82", "ANTO", Exchange.LSE, "Antofagasta plc", "GBP", "GB", "Materials", 16_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0007099541", "0709954", "PRU", Exchange.LSE, "Prudential plc", "GBP", "GB", "Financials", 32_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0006776081", "0677608", "PSON", Exchange.LSE, "Pearson plc", "GBP", "GB", "Consumer Discretionary", 8_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00B1YW4409", "B1YW440", "SSE", Exchange.LSE, "SSE plc", "GBP", "GB", "Utilities", 20_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00BVYVFW23", "BVYVFW2", "BNZL", Exchange.LSE, "Bunzl plc", "GBP", "GB", "Industrials", 12_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0008706128", "0870612", "LLOY", Exchange.LSE, "Lloyds Banking Group plc", "GBP", "GB", "Financials", 35_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00BD6K4575", "BD6K457", "CPG", Exchange.LSE, "Compass Group plc", "GBP", "GB", "Consumer Discretionary", 40_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("JE00B4T3BW64", "B4T3BW6", "GLEN", Exchange.LSE, "Glencore plc", "GBP", "GB", "Materials", 50_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0031215220", "3121522", "CRH", Exchange.LSE, "CRH plc", "GBP", "GB", "Materials", 42_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00B19NLV48", "B19NLV4", "EXPN", Exchange.LSE, "Experian plc", "GBP", "GB", "Industrials", 35_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00BDR05C01", "BDR05C0", "NWG", Exchange.LSE, "NatWest Group plc", "GBP", "GB", "Financials", 28_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0004835483", "0483548", "LAND", Exchange.LSE, "Land Securities Group plc", "GBP", "GB", "Real Estate", 5_500_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0031274896", "3127489", "MKS", Exchange.LSE, "Marks and Spencer Group plc", "GBP", "GB", "Consumer Discretionary", 7_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB00BPQY9420", "BPQY942", "AV.", Exchange.LSE, "Aviva plc", "GBP", "GB", "Financials", 14_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("GB0005603997", "0560399", "NG.", Exchange.LSE, "National Grid plc", "GBP", "GB", "Utilities", 45_000_000_000m, timeProvider));

        // US Equities — Top 10 (NYSE/NASDAQ)
        instruments.Add(CreateEquity("US0378331005", "2046251", "AAPL", Exchange.NASDAQ, "Apple Inc", "USD", "US", "Technology", 3_000_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US5949181045", "2588173", "MSFT", Exchange.NASDAQ, "Microsoft Corporation", "USD", "US", "Technology", 2_800_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US0231351067", "2000019", "AMZN", Exchange.NASDAQ, "Amazon.com Inc", "USD", "US", "Consumer Discretionary", 1_700_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US02079K3059", "BYVY8G0", "GOOGL", Exchange.NASDAQ, "Alphabet Inc Class A", "USD", "US", "Technology", 1_800_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US30303M1027", "B7TL820", "META", Exchange.NASDAQ, "Meta Platforms Inc", "USD", "US", "Technology", 1_200_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US67066G1040", "2379504", "NVDA", Exchange.NASDAQ, "NVIDIA Corporation", "USD", "US", "Technology", 2_500_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US88160R1014", "B616C79", "TSLA", Exchange.NASDAQ, "Tesla Inc", "USD", "US", "Consumer Discretionary", 800_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US46625H1005", "2190385", "JPM", Exchange.NYSE, "JPMorgan Chase & Co", "USD", "US", "Financials", 500_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US92826C8394", "B2PZN04", "V", Exchange.NYSE, "Visa Inc", "USD", "US", "Financials", 550_000_000_000m, timeProvider));
        instruments.Add(CreateEquity("US4781601046", "2475833", "JNJ", Exchange.NYSE, "Johnson & Johnson", "USD", "US", "Healthcare", 400_000_000_000m, timeProvider));

        // ETFs — 5 major
        instruments.Add(CreateEtf("IE00B3RBWM25", "B3RBWM2", "VWRL", Exchange.LSE, "Vanguard FTSE All-World UCITS ETF", "USD", "IE", "Multi-Asset", 12_000_000_000m, timeProvider));
        instruments.Add(CreateEtf("IE0005042456", "0504245", "ISF", Exchange.LSE, "iShares Core FTSE 100 UCITS ETF", "GBP", "IE", "UK Equity", 10_000_000_000m, timeProvider));
        instruments.Add(CreateEtf("IE00B5BMR087", "B5BMR08", "CSPX", Exchange.LSE, "iShares Core S&P 500 UCITS ETF", "USD", "IE", "US Equity", 65_000_000_000m, timeProvider));
        instruments.Add(CreateEtf("IE00B3XXRP09", "B3XXRP0", "VUSA", Exchange.LSE, "Vanguard S&P 500 UCITS ETF", "USD", "IE", "US Equity", 35_000_000_000m, timeProvider));
        instruments.Add(CreateEtf("IE00BFMNHK08", "BFMNHK0", "EQQQ", Exchange.LSE, "Invesco EQQQ NASDAQ-100 UCITS ETF", "USD", "IE", "US Technology", 8_000_000_000m, timeProvider));

        // Fixed Income — 5 UK gilts/bonds
        instruments.Add(CreateFixedIncome("GB00BDRHNP05", "BDRHNP0", "TG25", Exchange.LSE, "UK Gilt 4.25% 2025", "GBP", "GB", "Government Bonds",
            0.0425m, new DateTime(2025, 12, 7), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 6, 7), 100m, timeProvider));
        instruments.Add(CreateFixedIncome("GB00B24FF097", "B24FF09", "TG28", Exchange.LSE, "UK Gilt 5% 2028", "GBP", "GB", "Government Bonds",
            0.05m, new DateTime(2028, 3, 7), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 9, 7), 100m, timeProvider));
        instruments.Add(CreateFixedIncome("GB00B16NNR78", "B16NNR7", "TG30", Exchange.LSE, "UK Gilt 4.75% 2030", "GBP", "GB", "Government Bonds",
            0.0475m, new DateTime(2030, 12, 7), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 6, 7), 100m, timeProvider));
        instruments.Add(CreateFixedIncome("GB00BBJNQY21", "BBJNQY2", "TG36", Exchange.LSE, "UK Gilt 4.25% 2036", "GBP", "GB", "Government Bonds",
            0.0425m, new DateTime(2036, 3, 7), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 9, 7), 100m, timeProvider));
        instruments.Add(CreateFixedIncome("GB00B058DQ55", "B058DQ5", "TG55", Exchange.LSE, "UK Gilt 4.25% 2055", "GBP", "GB", "Government Bonds",
            0.0425m, new DateTime(2055, 12, 7), CouponFrequency.SemiAnnual, DayCountConvention.ActualActualIsda, new DateTime(2025, 6, 7), 100m, timeProvider));

        return instruments;
    }

    private static Instrument CreateEquity(
        string isin, string sedol, string ticker, Exchange exchange,
        string name, string currency, string country, string sector,
        decimal marketCap, TimeProvider timeProvider)
    {
        return Instrument.Create(isin, sedol, ticker, exchange, name, currency, country, sector, AssetClass.Equity, marketCap, timeProvider);
    }

    private static Instrument CreateEtf(
        string isin, string sedol, string ticker, Exchange exchange,
        string name, string currency, string country, string sector,
        decimal marketCap, TimeProvider timeProvider)
    {
        return Instrument.Create(isin, sedol, ticker, exchange, name, currency, country, sector, AssetClass.ETF, marketCap, timeProvider);
    }

    private static Instrument CreateFixedIncome(
        string isin, string sedol, string ticker, Exchange exchange,
        string name, string currency, string country, string sector,
        decimal couponRate, DateTime maturityDate, CouponFrequency couponFrequency,
        DayCountConvention dayCountConvention, DateTime lastCouponDate, decimal faceValue,
        TimeProvider timeProvider)
    {
        var instrument = Instrument.Create(isin, sedol, ticker, exchange, name, currency, country, sector, AssetClass.FixedIncome, 0m, timeProvider);
        var details = FixedIncomeDetails.Create(couponRate, maturityDate, couponFrequency, dayCountConvention, lastCouponDate, faceValue);
        instrument.SetFixedIncomeDetails(details, timeProvider);
        return instrument;
    }
}
