using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace VRT.MarketDepth.Wpf;

public partial class App : Application
{
    public static IServiceProvider Services => field ??= InitServices();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    private static IServiceProvider InitServices()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<MainWindow>()
            .AddSingleton<MainWindowViewModel>()
            .AddInfrastructure();

        return services.BuildServiceProvider();
    }
}
