using CSharpFunctionalExtensions.ValueTasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using VRT.MarketDepth.Extensions;

namespace VRT.MarketDepth.Services.GraGieldowa;

internal sealed class GraGieldowaMarketDataProvider : IMarketDataProvider
{
    private readonly IGraGieldowaDataApiClient _marketClient;
    private static IReadOnlyDictionary<string, GetMarketsResponseItem>? _marketsCache;
    public GraGieldowaMarketDataProvider(
        IGraGieldowaDataApiClient marketDataClient)
    {
        _marketClient = marketDataClient;
    }
    public async Task<Result<GetMarketDepthResponse>> GetMarketDepthAsync(GetMarketDepthRequest request, CancellationToken cancellationToken = default)
    {
        var markets = await GetMarkets();
        if (markets is null || markets.TryGetValue(request.Ticker, out var market) is false)
        {
            return Result.Failure<GetMarketDepthResponse>("Market not supported by GraGieldowa");
        }


        var result = await Result.Try(() => _marketClient.GetOrderBookHtml(market.InternalId!))
            .Ensure(r => r.IsSuccessStatusCode, r => $"{r.StatusCode} - {r.ReasonPhrase}")
            .Map(r => r.Content)
            .Map(ToGetMarketDepthResponse);
        return result;
    }
    public async Task<Result<GetMarketsResponse>> GetMarketsAsync(CancellationToken cancellationToken = default)
    {
        var markets = await GetMarkets();
        return markets is null
            ? Result.Failure<GetMarketsResponse>("Gra gieldowa has no markets")
            : new GetMarketsResponse(markets.Values.ToArray());
    }

    private GetMarketDepthResponse ToGetMarketDepthResponse(string? responseHtml)
    {
        if (string.IsNullOrWhiteSpace(responseHtml))
        {
            return GetMarketDepthResponse.Empty;
        }
        var html = new HtmlDocument();
        html.LoadHtml(responseHtml ?? "");

        var bids = html.DocumentNode
            .SelectNodes("//table[@id='arkusz_left']")
            .FirstOrDefault()?
            .SelectNodes(".//tbody/tr")
            .Select(ToGetMarketDepthResponseItem)
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();

        var asks = html.DocumentNode
            .SelectNodes("//table[@id='arkusz_right']")
            .FirstOrDefault()?
            .SelectNodes(".//tbody/tr")
            .Select(ToGetMarketDepthResponseItem)
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();

        var result = new GetMarketDepthResponse(bids ?? [], asks ?? []);
        return result;
    }
    private static GetMarketDepthResponseItem? ToGetMarketDepthResponseItem(HtmlNode? node)
    {
        if (node == null)
        {
            return null;
        }
        var fields = node.SelectNodes(".//td")?
            .Select(n => n.GetAttributeValue("data-text", ""))
            .ToArray() ?? [];

        return fields switch
        {
            [_, var volume, var value, ..] when volume.TryToDecimal(out var vol) && value.TryToDecimal(out var val) && vol > 0m
                => new(decimal.Round(val / vol, 4), vol),
            _ => null
        };
    }

    private async Task<IReadOnlyDictionary<string, GetMarketsResponseItem>?> GetMarkets()
    {
        if (_marketsCache != null)
        {
            return _marketsCache;
        }
        var result = await _marketClient.GetMarketsHtml();
        if (result.IsSuccessStatusCode is false)
        {
            return null;
        }
        var html = new HtmlDocument();
        html.LoadHtml(result.Content ?? "");
        var quotesTable = html.DocumentNode
            .SelectNodes("//table[@id='notowania_table']")
            .FirstOrDefault();
        if (quotesTable == null)
        {
            return null;
        }

        var rows = quotesTable.SelectNodes(".//tbody/tr/td/a")
            ?.Select(ToGetMarketsResponseItem)
            .Where(i => i?.InternalId is not null)
            .ToDictionary(k => k!.Ticker, k => k!);
        _marketsCache = rows;
        return rows;

    }
    private static GetMarketsResponseItem? ToGetMarketsResponseItem(HtmlNode? node)
    {
        if (node == null)
        {
            return null;
        }
        var ticker = node.GetAttributeValue("data-nazwa", "");
        var wid = node.GetAttributeValue("data-krotka", "")?.Split(",")?.LastOrDefault();
        if (string.IsNullOrWhiteSpace(ticker) || string.IsNullOrWhiteSpace(wid))
        {
            return null;
        }
        ticker = ticker.Trim('"', ' ', '\t', '\r', '\n');
        ticker = Regex.Replace(ticker, @"\s{2,}", " ", RegexOptions.NonBacktracking);
        return new GetMarketsResponseItem(ticker, "PLN", wid);
    }
}
