namespace VRT.MarketDepth.Extensions;

internal static class StringExtensions
{
    public static decimal ToDecimal(this string value)
    {
        return decimal.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
    }
}
