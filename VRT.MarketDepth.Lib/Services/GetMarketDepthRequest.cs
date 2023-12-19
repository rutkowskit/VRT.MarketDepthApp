namespace VRT.MarketDepth.Services;

public sealed record GetMarketDepthRequest(string Ticker, string CurrencySymbol);
