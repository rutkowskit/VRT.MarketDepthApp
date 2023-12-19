using VRT.MarketDepth.Extensions;

namespace VRT.MarketDepth.Services.Kanga;

internal sealed class KangaMarketDataProvider : IMarketDataProvider
{
    private readonly IKangaMarketDataApiClient _kangaMarketClient;
    public KangaMarketDataProvider(IKangaMarketDataApiClient kangaMarketClient)
    {
        _kangaMarketClient = kangaMarketClient;
    }
    public async Task<Result<GetMarketDepthResponse>> GetMarketDepthAsync(GetMarketDepthRequest request, CancellationToken cancellationToken = default)
    {
        var market = $"{request.Ticker}-{request.CurrencySymbol}";
        var result = await Result.Try(() => _kangaMarketClient.GetMarketDepth(new GetKangaMarketDepthRequest(market)))
            .Ensure(r => r.IsSuccessStatusCode, r => $"{r.StatusCode} - {r.ReasonPhrase}")
            .Map(r => r.Content)
            .Map(ToResponse);
        return result;
    }
    public async Task<Result<GetMarketsResponse>> GetMarketsAsync(CancellationToken cancellationToken = default)
    {
        var result = await Result.Try(() => _kangaMarketClient.GetMarkets())
            .Ensure(r => r.IsSuccessStatusCode, r => $"{r.StatusCode} - {r.ReasonPhrase}")
            .Map(r => r.Content)
            .Map(ToResponse);
        return result;
    }

    private GetMarketsResponse ToResponse(GetKangaMarketsResponse? response)
    {
        if (response is null)
        {
            return GetMarketsResponse.Empty;
        }
        var markets = response
            .Select(kv => kv.Key.Split('-'))
            .Where(k => k.Length == 2)
            .Select(k => new GetMarketsResponseItem(k[0], k[1]))
            .ToArray();
        return new GetMarketsResponse(markets);
    }

    private GetMarketDepthResponse ToResponse(GetKangaMarketDepthResponse? response)
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
    private GetMarketDepthResponseItem[] ToMarketDepthItems(GetKangaMarketDepthResponseItem[] responseItems)
    {
        if (responseItems is null || responseItems.Length == 0)
        {
            return [];
        }
        return responseItems
            .Select(i => new GetMarketDepthResponseItem(i.Price.ToDecimal(), i.Quantity.ToDecimal()))
            .ToArray();
    }


}
