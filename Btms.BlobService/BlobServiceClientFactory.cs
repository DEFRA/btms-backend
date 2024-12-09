using Azure.Storage.Blobs;
using Btms.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Btms.BlobService;

public class BlobServiceClientFactory(
    IServiceProvider serviceProvider, 
    IOptions<BlobServiceOptions> options,
    ILogger<BlobServiceClientFactory> logger,
    IHttpClientFactory? clientFactory = null)
    : AzureService(serviceProvider, logger, options.Value, clientFactory), IBlobServiceClientFactory
{
    public BlobServiceClient CreateBlobServiceClient(int timeout = default, int retries = default)
    {
        // Allow timeout and retry to be overridden, e.g. from healthchecker
        timeout = timeout > 0 ? timeout : options.Value.Timeout;
        retries = retries > 0 ? retries : options.Value.Retries;
        
        logger.LogInformation("CreateBlobServiceClient timeout={Timeout}, retries={Retries}.", timeout, retries);
        
        var bcOptions = new BlobClientOptions
        {
            Transport = Transport!,
            Retry =
            {
                MaxRetries = retries, NetworkTimeout = TimeSpan.FromSeconds(timeout)
            },
            Diagnostics = { IsLoggingContentEnabled = true, IsLoggingEnabled = true }
        };
        

        return new BlobServiceClient(
            new Uri(options.Value.DmpBlobUri),
            Credentials,
            bcOptions);
    }
}