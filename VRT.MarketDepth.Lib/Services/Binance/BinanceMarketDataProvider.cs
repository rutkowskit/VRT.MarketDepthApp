using VRT.MarketDepth.Extensions;

namespace VRT.MarketDepth.Services.Binance;

internal sealed class BinanceMarketDataProvider : IMarketDataProvider
{
    private readonly IBinanceMarketDataApiClient _apiClient;
    public BinanceMarketDataProvider(IBinanceMarketDataApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    public async Task<Result<GetMarketDepthResponse>> GetMarketDepthAsync(GetMarketDepthRequest request, CancellationToken cancellationToken = default)
    {
        var market = $"{request.Ticker}{request.CurrencySymbol}";
        var result = await Result.Try(() => _apiClient.GetMarketDepth(new GetBinanceMarketDepthRequest(market, 100)))
            .Ensure(r => r.IsSuccessStatusCode, r => $"{r.StatusCode} - {r.ReasonPhrase}")
            .Map(r => r.Content)
            .Map(ToResponse);
        return result;
    }
    public async Task<Result<GetMarketsResponse>> GetMarketsAsync(CancellationToken cancellationToken = default)
    {
        var result = await Result.Try(() => _apiClient.GetMarkets())
            .Ensure(r => r.IsSuccessStatusCode, r => $"{r.StatusCode} - {r.ReasonPhrase}")
            .Map(r => r.Content)
            .Map(ToResponse);
        return result;
    }

    private GetMarketsResponse ToResponse(GetBinanceMarketsResponse? response)
    {
        if (response is null)
        {
            return GetMarketsResponse.Empty;
        }
        var markets = response.Symbols
            .Select(s => new GetMarketsResponseItem(s.BaseAsset, s.QuoteAsset))
            .ToArray();
        return new GetMarketsResponse(markets);
    }
    private GetMarketDepthResponse ToResponse(GetBinanceMarketDepthResponse? response)
    {
        if (response is null)
        {
            return GetMarketDepthResponse.Empty;
        }
        var bids = ToMarketDepthItems(response.Bids);
        var asks = ToMarketDepthItems(response.Asks);
        var result = new GetMarketDepthResponse(bids, asks);
        return result;
    }
    private GetMarketDepthResponseItem[] ToMarketDepthItems(string[][]? items)
    {
        if (items is null || items.Length == 0)
        {
            return [];
        }
        return items
            .Select(i => new GetMarketDepthResponseItem(i[0].ToDecimal(), i[1].ToDecimal()))
            .ToArray();
    }
}
