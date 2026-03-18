namespace VRT.MarketDepth.Extensions;

internal static class StringExtensions
{
    public static decimal ToDecimal(this string value)
    {
        return value.TryToDecimal(out var result)
            ? result
            : 0m;
    }

    public static bool TryToDecimal(this string value, out decimal result)
        => decimal.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out result);
}
