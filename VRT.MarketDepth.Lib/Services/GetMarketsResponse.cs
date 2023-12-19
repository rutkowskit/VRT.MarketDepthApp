namespace VRT.MarketDepth.Services;

public record GetMarketsResponse(IReadOnlyCollection<GetMarketsResponseItem> Markets)
{
    public static readonly GetMarketsResponse Empty = new([]);
}

public sealed record GetMarketsResponseItem(string Ticker, string VsTicker, string? InternalId = null)
{
    public override string ToString() => $"{Ticker}-{VsTicker}";
}
