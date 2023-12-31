using System.Windows;
using System.Windows.Input;

namespace VRT.MarketDepth.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Focusable = true;
        Loaded += (s, e) => Keyboard.Focus(this);
    }
}