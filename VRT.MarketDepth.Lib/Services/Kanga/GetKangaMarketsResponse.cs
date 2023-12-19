using System.Text.Json.Serialization;

namespace VRT.MarketDepth.Services.Kanga;

internal sealed class GetKangaMarketsResponse : Dictionary<string, GetKangaMarketsResponseValue>;

internal sealed class GetKangaMarketsResponseValue
{
    [JsonPropertyName("quote_volume")]
    required public string QuoteVolute { get; init; }
    [JsonPropertyName("base_volume")]
    required public string BaseVolume { get; init; }
    [JsonPropertyName("last_price")]
    required public string LastPrice { get; init; }
}
