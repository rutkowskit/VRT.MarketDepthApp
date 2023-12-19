
using System.Windows.Controls;


namespace VRT.MarketDepth.Wpf.Controls;


[DependencyPropertyGenerator.DependencyProperty("MarketName", typeof(string))]
[DependencyPropertyGenerator.DependencyProperty("MarketDepth", typeof(GetMarketDepthResponse))]
[DependencyPropertyGenerator.DependencyProperty("MaxSideCount", typeof(int))]
public partial class MarketDepthChart : UserControl
{
    public MarketDepthChart()
    {
        InitializeComponent();
    }

    partial void OnMarketDepthChanged(GetMarketDepthResponse? newValue)
    {
        var topItems = Math.Max(20, MaxSideCount);
        uxChart.Plot.Clear();

        var bidValues = (newValue?.Bids?
            .OrderBy(a => a.Price)
            .Select(a => (Label: a.Price.ToString(), Value: (double)(a.Price * a.Quantity)))
            .TakeLast(topItems)
            .ToArray()) ?? [];

        var askValues = (newValue?.Asks?
            .OrderBy(a => a.Price)
            .Select(a => (Label: a.Price.ToString(), Value: (double)(a.Price * a.Quantity)))
            .Take(topItems)
            .ToArray()) ?? [];

        var positions = Enumerable
            .Range(0, askValues.Length + bidValues.Length)
            .Select(v => (double)v)
            .ToArray();

        var labels = new List<string>();

        if (bidValues.Length > 0)
        {
            labels.AddRange(bidValues.Select(v => v.Label));
            var bidBar = uxChart.Plot.AddBar(bidValues.Select(v => v.Value).ToArray(), positions.Take(bidValues.Length).ToArray(), System.Drawing.Color.Green);
        }
        if (askValues.Length > 0)
        {
            labels.AddRange(askValues.Select(v => v.Label));
            var asksBar = uxChart.Plot.AddBar(askValues.Select(v => v.Value).ToArray(), positions.Skip(bidValues.Length).ToArray(), System.Drawing.Color.Red);
        }
        if (positions.Length > 0)
        {
            uxChart.Plot.XTicks(positions, labels.ToArray());
            uxChart.Plot.XAxis.TickLabelStyle(rotation: 45);
        }
        uxChart.RefreshRequest();
    }

    partial void OnMarketNameChanged(string? newValue)
    {
        uxChart.Plot.Title(newValue ?? "");
        uxChart.RefreshRequest();
    }
}
