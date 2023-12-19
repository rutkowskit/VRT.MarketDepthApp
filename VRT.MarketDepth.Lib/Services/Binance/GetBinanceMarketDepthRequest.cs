namespace VRT.MarketDepth.Services.Binance;

internal sealed record GetBinanceMarketDepthRequest(string Market, int Limit = 50);
