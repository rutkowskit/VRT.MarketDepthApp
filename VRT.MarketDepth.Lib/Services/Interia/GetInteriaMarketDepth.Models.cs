using System.Text.Json.Serialization;

namespace VRT.MarketDepth.Services.Interia;

internal sealed record GetInteriaMarketDepthRequest(string WalorId);

internal sealed class GetInteriaMarketDepthResponse : List<GetInteriaMarketDepthResponseItem>;

internal class GetInteriaMarketDepthResponseItem
{
    [JsonPropertyName("walor_id")]
    public int WalorId { get; init; }
    [JsonPropertyName("strona_zlec")]
    required public string StronaZlec { get; init; }
    [JsonPropertyName("czas_wprow")]
    required public string CzasWprow { get; init; }
    [JsonPropertyName("kurs")]
    public decimal Kurs { get; init; }

    [JsonPropertyName("wolumen")]
    public int Wolumen { get; init; }
}
