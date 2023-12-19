using System.Text.Json.Serialization;

namespace VRT.MarketDepth.Services.Kanga;

internal class GetKangaMarketDepthResponse
{
    [JsonPropertyName("timestamp")]
    required public string Timestamp { get; init; }
    [JsonPropertyName("bids")]
    public GetKangaMarketDepthResponseItem[] Bids { get; init; } = [];
    [JsonPropertyName("asks")]
    public GetKangaMarketDepthResponseItem[] Asks { get; init; } = [];
}
internal class GetKangaMarketDepthResponseItem
{
    [JsonPropertyName("quantity")]
    required public string Quantity { get; init; }
    [JsonPropertyName("price")]
    required public string Price { get; init; }
}
