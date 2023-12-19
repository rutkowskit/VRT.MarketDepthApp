using Refit;

namespace VRT.MarketDepth.Services.Interia;

internal interface IInteriaWebClient
{
    [Get("/gieldy/notowania-gpw")]
    Task<ApiResponse<string>> GetGpwQuotesHtml();
}
