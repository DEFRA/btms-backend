using Azure.Core;
using Btms.Azure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

namespace Btms.Azure;

/// <summary>
/// Takes care of retriving a token via ConfidentialClientApplicationBuilder
/// which allows us to inject our CDP_HTTPS_PROXY based http client.
///
/// It's unclear why this isn't available out of the box!
/// - IMsalHttpClientFactory isn't used by ClientSecretCredential
/// - The ClientSecretCredential has an internal constructor accepting MsalConfidentialClient but nothing seems to use it
/// - MsalConfidentialClient is itself internal 
/// </summary>
public class ConfidentialClientApplicationTokenCredential : TokenCredential
{
    private readonly string[] _scopes = ["https://storage.azure.com/.default"];

    private readonly IConfidentialClientApplication _app;
    public ConfidentialClientApplicationTokenCredential(IServiceProvider serviceProvider, IAzureConfig config)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<MsalHttpClientFactoryAdapter>();

        _app = ConfidentialClientApplicationBuilder.Create(config.AzureClientId)
            .WithHttpClientFactory(httpClientFactory)
            .WithTenantId(config.AzureTenantId)
            .WithClientSecret(config.AzureClientSecret)
            .Build();
    }
    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var authResult = _app.AcquireTokenForClient(_scopes).ExecuteAsync(cancellationToken).Result;
        return ValueTask.FromResult(new AccessToken(authResult.AccessToken, authResult.ExpiresOn));
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var authResult = _app.AcquireTokenForClient(_scopes).ExecuteAsync(cancellationToken).Result;
        return new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
    }
}