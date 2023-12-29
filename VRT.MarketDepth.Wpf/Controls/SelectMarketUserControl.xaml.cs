using DependencyPropertyGenerator;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Input;

namespace VRT.MarketDepth.Wpf.Controls;

[DependencyProperty<GetMarketsResponseItem[]>("Markets")]
[DependencyProperty<GetMarketsResponseItem>("SelectedMarket")]
[DependencyProperty<string>("FilterText")]
[DependencyProperty<GetMarketsResponseItem[]>("FilteredMarkets", IsReadOnly = true)]
[DependencyProperty<ICommand>("OkCommand")]
[DependencyProperty<ICommand>("CancelCommand")]
public partial class SelectMarketUserControl : UserControl
{
    private GetMarketsResponseItem[] _allMarkets;
    public SelectMarketUserControl()
    {
        _allMarkets = [];
        InitializeComponent();
    }

    [RelayCommand(CanExecute = nameof(CanConfirmSelection))]
    private void ConfirmSelection()
    {
        OkCommand?.Execute(SelectedMarket);
        DialogHost.CloseDialogCommand.Execute(SelectedMarket, null);
    }

    private bool CanConfirmSelection()
    {
        return SelectedMarket is not null;
    }
    partial void OnSelectedMarketChanged(GetMarketsResponseItem? newValue)
    {
        confirmSelectionCommand?.NotifyCanExecuteChanged();
    }
    partial void OnMarketsChanged(GetMarketsResponseItem[]? newValue)
    {
        _allMarkets = newValue ?? [];
        ApplyMarketsFiltering();
    }
    partial void OnFilterTextChanged(string? newValue)
    {
        ApplyMarketsFiltering();
    }
    private void ApplyMarketsFiltering()
    {
        var parts = FilterText?.Split() ?? [];
        var items = _allMarkets;
        if (parts.Length > 0)
        {
            items = items
                .Where(i => parts.All(p => i.ToString().Contains(p, StringComparison.InvariantCultureIgnoreCase)))
                .ToArray();
        }
        FilteredMarkets = items;
        SelectedMarket = items.FirstOrDefault();
    }

    private void OnMarketDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CanConfirmSelection())
        {
            ConfirmSelectionCommand.Execute(null);
        }
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (CanConfirmSelection())
            {
                ConfirmSelectionCommand.Execute(null);
            }
        }
        base.OnKeyDown(e);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (FilteredMarkets is null)
        {
            return;
        }
        var markets = FilteredMarkets!;
        if (e.Key == Key.Down)
        {
            var nextIndex = Array.IndexOf(markets, SelectedMarket) + 1;
            if (nextIndex >= markets.Length || nextIndex < 0)
            {
                nextIndex = Math.Max(0, markets.Length - 1);
            }
            SelectMarketAt(nextIndex);
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            var nextIndex = Math.Max(0, Array.IndexOf(markets, SelectedMarket) - 1);
            SelectMarketAt(nextIndex);
            e.Handled = true;
        }
    }
    private void SelectMarketAt(int index)
    {
        SelectedMarket = FilteredMarkets?.ElementAtOrDefault(index);
        uxMarketsList.ScrollIntoView(SelectedMarket);
    }
}
