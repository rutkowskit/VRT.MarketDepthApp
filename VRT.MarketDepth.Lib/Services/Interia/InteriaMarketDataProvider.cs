using CSharpFunctionalExtensions.ValueTasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace VRT.MarketDepth.Services.Interia;

internal sealed class InteriaMarketDataProvider : IMarketDataProvider
{
    private readonly IInteriaMarketDataApiClient _marketClient;
    private static IReadOnlyDictionary<string, GetMarketsResponseItem>? _marketsCache;
    private readonly IInteriaWebClient _webClient;
    public InteriaMarketDataProvider(IInteriaMarketDataApiClient interiaMarketClient, IInteriaWebClient webClient)
    {
        _marketClient = interiaMarketClient;
        _webClient = webClient;
    }
    public async Task<Result<GetMarketDepthResponse>> GetMarketDepthAsync(GetMarketDepthRequest request, CancellationToken cancellationToken = default)
    {
        var markets = await GetMarkets();
        if (markets is null || markets.TryGetValue(request.Ticker, out var market) is false)
        {
            return Result.Failure<GetMarketDepthResponse>("Market not supported by Interia");
        }


        var result = await Result.Try(() => _marketClient.GetMarketDepth(new GetInteriaMarketDepthRequest(market.InternalId!)))
            .Ensure(r => r.IsSuccessStatusCode, r => $"{r.StatusCode} - {r.ReasonPhrase}")
            .Map(r => r.Content)
            .Map(ToResponse);
        return result;
    }
    public async Task<Result<GetMarketsResponse>> GetMarketsAsync(CancellationToken cancellationToken = default)
    {
        var markets = await GetMarkets();
        return markets is null
            ? Result.Failure<GetMarketsResponse>("Interia has no markets")
            : new GetMarketsResponse(markets.Values.ToArray());
    }

    private GetMarketDepthResponse ToResponse(GetInteriaMarketDepthResponse? response)
    {
        if (response is null || response.Count == 0)
        {
            return GetMarketDepthResponse.Empty;
        }
        var grouped = response
            .GroupBy(i => i.StronaZlec)
            .ToDictionary(k => k.Key, v => v.ToArray());

        GetMarketDepthResponseItem[] resultBids = [];
        GetMarketDepthResponseItem[] resultAsks = [];

        if (grouped.TryGetValue("V", out var asks))
        {
            resultAsks = asks.Select(a => new GetMarketDepthResponseItem(decimal.Round(a.Kurs), a.Wolumen)).ToArray();
        }
        if (grouped.TryGetValue("A", out var bids))
        {
            resultBids = bids.Select(a => new GetMarketDepthResponseItem(decimal.Round(a.Kurs, 2), a.Wolumen)).ToArray();
        }
        var result = new GetMarketDepthResponse(resultBids, resultAsks);
        return result;
    }

    private async Task<IReadOnlyDictionary<string, GetMarketsResponseItem>?> GetMarkets()
    {
        if (_marketsCache != null)
        {
            return _marketsCache;
        }
        var result = await _webClient.GetGpwQuotesHtml();
        if (result.IsSuccessStatusCode is false)
        {
            return null;
        }
        var html = new HtmlDocument();
        html.LoadHtml(result.Content);
        var quotesTable = html.DocumentNode
            .SelectNodes("//table[@class='business-table-standard-table']")
            .FirstOrDefault(n => n.OuterHtml.Contains("business-table-stock-quotes", StringComparison.InvariantCultureIgnoreCase));
        if (quotesTable == null)
        {
            return null;
        }

        var rows = quotesTable.SelectNodes(".//tbody/tr")
            ?.Select(ToGetMarketsResponseItem)
            .Where(i => i?.InternalId is not null)
            .ToDictionary(k => k!.Ticker, k => k!);
        return rows;

    }
    private static GetMarketsResponseItem? ToGetMarketsResponseItem(HtmlNode? node)
    {
        if (node == null)
        {
            return null;
        }
        var firstCol = node.SelectSingleNode(".//td");
        var ticker = firstCol.SelectSingleNode(".//p").InnerText;
        var wid = firstCol.GetAttributeValue("data-row-href", "")?.Split(",")?.LastOrDefault();
        if (string.IsNullOrWhiteSpace(ticker) || string.IsNullOrWhiteSpace(wid))
        {
            return null;
        }
        ticker = ticker.Trim('"', ' ', '\t', '\r', '\n');
        ticker = Regex.Replace(ticker, @"\s{2,}", " ", RegexOptions.NonBacktracking);
        return new GetMarketsResponseItem(ticker, "PLN", wid);
    }
}
