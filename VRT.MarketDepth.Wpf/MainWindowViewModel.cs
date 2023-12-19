using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpFunctionalExtensions;


namespace VRT.MarketDepth.Wpf;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private const long DefaultRefreshIntervalMs = 3000;
    private const long MinRefreshIntervalMs = 500;
    private readonly IMarketDataProvider[] _marketDataProvider;

    [ObservableProperty]
    private GetMarketDepthResponse? _marketDepthData;

    [ObservableProperty]
    private string? _marketName;

    [ObservableProperty]
    private GetMarketsResponseItem? _selectedMarket;

    [ObservableProperty]
    private GetMarketsResponseItem[]? _markets;

    [ObservableProperty]
    private int _topItemsCount = 20;

    [ObservableProperty]
    private long _refreshIntervalMs = DefaultRefreshIntervalMs;

    public MainWindowViewModel(IEnumerable<IMarketDataProvider> marketData)
    {
        _marketDataProvider = marketData?.ToArray() ?? [];
        LoadValuesFromSettings();
        _ = LoadMarkets(CancellationToken.None);
    }

    [RelayCommand]
    private async Task GetMarketDepth(CancellationToken cancellationToken)
    {
        if (SelectedMarket is null)
        {
            return;
        }
        var request = new GetMarketDepthRequest(SelectedMarket.Ticker, SelectedMarket.VsTicker);

        var data = await _marketDataProvider
            .Select(s => Result.Success()
                .BindTry(() => s.GetMarketDepthAsync(request, cancellationToken))
                .Compensate(err => GetMarketDepthResponse.Empty))
            .Combine()
            .Map(s => s.Aggregate((a, b) => a + b))
            .Map(s => CombineNearValues(s, 0.001m))
            .Tap(_ => MarketName = SelectedMarket.ToString());

        data
            .Tap(d => MarketDepthData = d)
            .Tap(_ => SaveValuesToSettings());
    }

    [RelayCommand]
    private async Task LoadMarkets(CancellationToken cancellationToken)
    {
        var data = await _marketDataProvider
            .Select(s => Result.Success()
                .BindTry(() => s.GetMarketsAsync(cancellationToken))
                .Compensate(err => GetMarketsResponse.Empty))
            .Combine()
            .Map(s => s.SelectMany(m => m.Markets))
            .Tap(d => Markets = d.Distinct().ToArray());
    }

    [RelayCommand]
    private void SelectMarket(GetMarketsResponseItem market)
    {
        SelectedMarket = market;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task StartGettingMarketDepths(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            await GetMarketDepthCommand.ExecuteAsync(null);
            var refreshInterval = (int)Math.Max(MinRefreshIntervalMs, RefreshIntervalMs);
            await Task.Delay(refreshInterval, cancellationToken).ContinueWith(delegate { }, CancellationToken.None);
        }
    }

    private static GetMarketDepthResponse CombineNearValues(GetMarketDepthResponse response, decimal deltaPercentage)
    {
        var combinedBids = CombineNearValues(response.Bids, deltaPercentage).ToArray();
        var combinedAsks = CombineNearValues(response.Asks, deltaPercentage).ToArray();
        return new GetMarketDepthResponse(combinedBids, combinedAsks);
    }

    private static IEnumerable<GetMarketDepthResponseItem> CombineNearValues(
        GetMarketDepthResponseItem[] items,
        decimal maxDeltaPercentage)
    {
        var query = items.OrderBy(i => i.Price);
        GetMarketDepthResponseItem? aggregator = null;
        decimal groupMaxValue = 0;
        foreach (var item in query)
        {
            if (aggregator == null)
            {
                aggregator = item;
                groupMaxValue = item.Price * (1 + maxDeltaPercentage);
                continue;
            }
            if (item.Price <= groupMaxValue)
            {
                aggregator = new GetMarketDepthResponseItem(aggregator.Price, item.Quantity + aggregator.Quantity);
            }
            else
            {
                yield return aggregator;
                aggregator = item;
                groupMaxValue = item.Price * (1 + maxDeltaPercentage);
            }

        }
        if (aggregator is not null)
        {
            yield return aggregator;
        }
    }

    private void LoadValuesFromSettings()
    {
        SelectedMarket = new GetMarketsResponseItem(Settings.Default.Ticker, Settings.Default.VsTicker);
        TopItemsCount = Settings.Default.Top;
        RefreshIntervalMs = Settings.Default.RefreshIntervalMs;
    }
    private void SaveValuesToSettings()
    {
        Settings.Default.Ticker = SelectedMarket?.Ticker;
        Settings.Default.VsTicker = SelectedMarket?.VsTicker;
        Settings.Default.Top = TopItemsCount;
        Settings.Default.RefreshIntervalMs = RefreshIntervalMs;
        Settings.Default.Save();
    }
}
