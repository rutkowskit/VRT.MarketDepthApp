using System.Text.Json.Serialization;

namespace VRT.MarketDepth.Services.Binance;

internal sealed class GetBinanceMarketDepthResponse
{
    [JsonPropertyName("lastUpdateId")]
    public long LastUpdateId { get; set; }
    [JsonPropertyName("bids")]
    public string[][]? Bids { get; set; }
    [JsonPropertyName("asks")]
    public string[][]? Asks { get; set; }
}
