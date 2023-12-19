using Refit;

namespace VRT.MarketDepth.Services.Kanga;

internal interface IKangaMarketDataApiClient
{
    [Get("/api/v2/market/depth?market={request.Market}")]
    Task<ApiResponse<GetKangaMarketDepthResponse>> GetMarketDepth(GetKangaMarketDepthRequest request);

    [Get("/api/v2/market/ticker")]
    Task<ApiResponse<GetKangaMarketsResponse>> GetMarkets();
}
