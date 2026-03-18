using Refit;

namespace VRT.MarketDepth.Services.GraGieldowa;

internal interface IGraGieldowaDataApiClient
{
    [Get("/notowania/akcje/GR_GPW_NC")]
    Task<ApiResponse<string>> GetMarketsHtml();

    [Get("/spolka_arkusz_zl/spolka/{ShortName}")]
    Task<ApiResponse<string>> GetOrderBookHtml(string shortName);
}
