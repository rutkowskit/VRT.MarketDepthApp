namespace VRT.MarketDepth.Services;

public interface IMarketDataProvider
{
    Task<Result<GetMarketDepthResponse>> GetMarketDepthAsync(GetMarketDepthRequest request, CancellationToken cancellationToken = default);
    Task<Result<GetMarketsResponse>> GetMarketsAsync(CancellationToken cancellationToken = default);
}

