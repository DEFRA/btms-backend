
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

namespace Btms.Azure.Extensions;

/// <summary>
/// The Azure SDK doesn't use the CDP proxied Http Client by default, so previously we used the HTTPS_CLIENT env var to
/// send the requests via CDPs squid proxy. This code is intended to use the http client we already setup that uses the proxy
/// when the CDP_HTTPS_PROXY env var is set.
/// Code borrowed from https://anthonysimmon.com/overriding-msal-httpclient-with-ihttpclientfactory/
/// </summary>
/// <param name="httpClientFactory"></param>
public class MsalHttpClientFactoryAdapter(IHttpClientFactory httpClientFactory) : IMsalHttpClientFactory
{
    public HttpClient GetHttpClient()
    {
        return httpClientFactory.CreateClient("Msal");
    }
}

public static class ServiceExtensions
{
    public static void AddMsalHttpProxyClient(this IServiceCollection services, Func<IServiceProvider, HttpClientHandler> configurePrimaryHttpMessageHandler)
    {
        // Dependency injection registration
        services.AddHttpClient("Msal")
            .ConfigurePrimaryHttpMessageHandler(configurePrimaryHttpMessageHandler)
            .ConfigureHttpClient(httpClient =>
            {
                // Default MSAL settings:
                // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/4.61.3/src/client/Microsoft.Identity.Client/Http/HttpClientConfig.cs#L18-L20
                httpClient.MaxResponseContentBufferSize = 1024 * 1024;
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

        services.AddSingleton<MsalHttpClientFactoryAdapter>();

        // services.AddSingleton(sp => new MsalCl
    }
}