using System.Text.Json.Serialization;

namespace VRT.MarketDepth.Services.Binance;

internal sealed class GetBinanceMarketsResponse
{
    [JsonPropertyName("symbols")]
    public GetBinanceMarketsResponseSymbol[] Symbols { get; init; } = [];
}

internal class GetBinanceMarketsResponseSymbol
{
    [JsonPropertyName("symbol")]
    required public string Symbol { get; init; }
    [JsonPropertyName("baseAsset")]
    required public string BaseAsset { get; init; }
    [JsonPropertyName("quoteAsset")]
    required public string QuoteAsset { get; init; }
}
