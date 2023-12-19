using CommunityToolkit.Mvvm.Input;
using DependencyPropertyGenerator;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Input;
using VRT.MarketDepth.Services;

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
        FilteredMarkets = FilterMarkets(_allMarkets).ToArray();
    }
    partial void OnFilterTextChanged(string? newValue)
    {
        FilteredMarkets = FilterMarkets(_allMarkets).ToArray();
    }
    private GetMarketsResponseItem[] FilterMarkets(GetMarketsResponseItem[] items)
    {
        var parts = FilterText?.Split() ?? [];
        if (parts.Length == 0)
        {
            return items;
        }
        return items
            .Where(i => parts.All(p => i.ToString().Contains(p, StringComparison.InvariantCultureIgnoreCase)))
            .ToArray();
    }
}
