using Refit;

namespace VRT.MarketDepth.Services.Interia;

internal interface IInteriaMarketDataApiClient
{
    [Get("/business/walor/getBestOrders/json?wId={request.WalorId}")]
    Task<ApiResponse<GetInteriaMarketDepthResponse>> GetMarketDepth(GetInteriaMarketDepthRequest request);
}
