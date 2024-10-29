using Azure.Storage.Blobs;
using Cdms.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cdms.BlobService;

public class BlobServiceClientFactory(
    IOptions<BlobServiceOptions> options,
    ILogger<BlobServiceClientFactory> logger,
    IHttpClientFactory? clientFactory = null)
    : AzureService<BlobServiceClientFactory>(logger, options.Value, clientFactory), IBlobServiceClientFactory
{
    public BlobServiceClient CreateBlobServiceClient()
    {
        var bcOptions = new BlobClientOptions
        {
            Transport = Transport!,
            Retry =
            {
                MaxRetries = options.Value.Retries, NetworkTimeout = TimeSpan.FromSeconds(options.Value.Timeout)
            },
            Diagnostics = { IsLoggingContentEnabled = true, IsLoggingEnabled = true }
        };

        return new BlobServiceClient(
            new Uri(options.Value.DmpBlobUri),
            Credentials,
            bcOptions);
    }
}