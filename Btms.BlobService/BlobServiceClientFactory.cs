using Azure.Core;
using Azure.Core.Diagnostics;
using Azure.Storage.Blobs;
using Btms.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.Tracing;
using Azure.Core.Pipeline;
using Azure.Identity;

namespace Btms.BlobService;

public class BlobServiceClientFactory(
    IServiceProvider serviceProvider,
    IOptions<BlobServiceOptions> defaultOptions,
    ILogger<BlobServiceClientFactory> logger,
    IHttpClientFactory? clientFactory = null)
    : IBlobServiceClientFactory
{
    public BlobServiceClient CreateBlobServiceClient(int timeout = default, int retries = default)
    {
        return CreateBlobServiceClient(defaultOptions);
    }
    public BlobServiceClient CreateBlobServiceClient(IOptions<IBlobServiceOptions> options, int timeout = default, int retries = default)
    {
        // Allow timeout and retry to be overridden, e.g. from healthchecker
        timeout = timeout > 0 ? timeout : options.Value.Timeout;
        retries = retries > 0 ? retries : options.Value.Retries;

        logger.LogInformation("CreateBlobServiceClient timeout={Timeout}, retries={Retries}", timeout, retries);

        var bcOptions = new BlobClientOptions
        {
            Transport = BuildTransport()!,
            Retry =
            {
                MaxRetries = retries, NetworkTimeout = TimeSpan.FromSeconds(timeout)
            },
            Diagnostics = { IsLoggingContentEnabled = true, IsLoggingEnabled = true }
        };

        return new BlobServiceClient(
            new Uri(options.Value.DmpBlobUri),
            BuildCredentials(options),
            bcOptions);
    }

    private HttpClientTransport? BuildTransport()
    {
        if (clientFactory != null)
        {
            return new HttpClientTransport(clientFactory.CreateClient("proxy"));
        }

        return null;
    }

    private TokenCredential BuildCredentials(IOptions<IBlobServiceOptions> options)
    {
        TokenCredential? credentials;
        if (options.Value.AzureClientId != null)
        {
            logger.LogInformation("Creating azure credentials based on config vars for {AzureClientId}",
                options.Value.AzureClientId);

            credentials =
                new ConfidentialClientApplicationTokenCredential(serviceProvider, options.Value);

            logger.LogInformation("Created azure credentials");
        }
        else
        {
            logger.LogInformation(
                "Creating azure credentials using default creds because AZURE_CLIENT_ID env var not found");
            credentials = new DefaultAzureCredential();
            logger.LogInformation("Created azure default credentials");
        }

        return credentials;
    }
}