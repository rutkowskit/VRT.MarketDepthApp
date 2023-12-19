using Refit;

namespace VRT.MarketDepth.Services.Binance;

internal interface IBinanceMarketDataApiClient
{
    [Get("/api/v3/depth?symbol={request.Market}&limit={request.Limit}")]
    Task<ApiResponse<GetBinanceMarketDepthResponse>> GetMarketDepth(GetBinanceMarketDepthRequest request);

    [Get("/api/v3/exchangeInfo?permissions=SPOT")]
    Task<ApiResponse<GetBinanceMarketsResponse>> GetMarkets();
}
