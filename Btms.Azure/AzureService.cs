using System.Diagnostics.Tracing;
using Azure.Core;
using Azure.Core.Diagnostics;
using Azure.Core.Pipeline;
using Azure.Identity;
using Btms.Azure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Btms.Azure;


public abstract class AzureService
{
    protected readonly TokenCredential Credentials;
    protected readonly HttpClientTransport? Transport;
    protected readonly ILogger Logger;

    protected AzureService(IServiceProvider serviceProvider, ILogger logger, IAzureConfig config, IHttpClientFactory? clientFactory = null)
    {
        Logger = logger;
        using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);

        if (config.AzureClientId != null)
        {
            logger.LogDebug("Creating azure credentials based on config vars for {AzureClientId}",
                config.AzureClientId);
            
            Credentials =
                new ConfidentialClientApplicationTokenCredential(serviceProvider, config);
            
            logger.LogDebug("Created azure credentials");
        }
        else
        {
            logger.LogDebug(
                "Creating azure credentials using default creds because AZURE_CLIENT_ID env var not found.");
            Credentials = new DefaultAzureCredential();
            logger.LogDebug("Created azure default credentials");
        }

        if (clientFactory != null)
        {
            Transport = new HttpClientTransport(clientFactory.CreateClient("proxy"));
        }
    }
}