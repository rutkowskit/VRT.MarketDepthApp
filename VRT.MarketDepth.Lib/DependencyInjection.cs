using Microsoft.Extensions.DependencyInjection;
using Refit;
using VRT.MarketDepth.Services;
using VRT.MarketDepth.Services.Binance;
using VRT.MarketDepth.Services.GraGieldowa;
using VRT.MarketDepth.Services.Interia;
using VRT.MarketDepth.Services.Kanga;

namespace VRT.MarketDepth;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
           .AddTransient<IMarketDataProvider, KangaMarketDataProvider>()
           .AddTransient<IMarketDataProvider, BinanceMarketDataProvider>()
           //.AddTransient<IMarketDataProvider, InteriaMarketDataProvider>() //doesn't work at the moment
           .AddTransient<IMarketDataProvider, GraGieldowaMarketDataProvider>();


        services
            .AddRefitClient<IKangaMarketDataApiClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.kanga.exchange"));

        services
            .AddRefitClient<IBinanceMarketDataApiClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.binance.com"));

        services
            .AddRefitClient<IInteriaMarketDataApiClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.interia.pl"));

        services
            .AddRefitClient<IInteriaWebClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://biznes.interia.pl"));

        services
            .AddRefitClient<IGraGieldowaDataApiClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://gragieldowa.pl"));
        return services;
    }
}
