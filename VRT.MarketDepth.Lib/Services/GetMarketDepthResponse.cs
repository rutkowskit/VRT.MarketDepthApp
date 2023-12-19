namespace VRT.MarketDepth.Services;

public sealed record GetMarketDepthResponse(GetMarketDepthResponseItem[] Bids, GetMarketDepthResponseItem[] Asks)
{
    public static readonly GetMarketDepthResponse Empty = new([], []);
    public static GetMarketDepthResponse operator +(GetMarketDepthResponse a, GetMarketDepthResponse b)
    {
        if (a is null || b is null)
        {
            return a ?? b ?? Empty;
        }
        return new GetMarketDepthResponse(Combine(a.Bids, b.Bids), Combine(a.Asks, b.Asks));
    }
    private static GetMarketDepthResponseItem[] Combine(GetMarketDepthResponseItem[] a, GetMarketDepthResponseItem[] b)
    {
        var result = a.Concat(b)
            .GroupBy(b => b.Price)
            .Select(b => new GetMarketDepthResponseItem(b.Key, b.Sum(x => x.Quantity)))
            .ToArray();
        return result;
    }
}

public sealed record GetMarketDepthResponseItem(decimal Price, decimal Quantity);