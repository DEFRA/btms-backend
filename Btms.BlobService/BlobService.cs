using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Btms.Azure;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace Btms.BlobService;

public class BlobService(
    IServiceProvider serviceProvider,
    IBlobServiceClientFactory blobServiceClientFactory,
    ILogger<BlobService> logger,
    IOptions<BlobServiceOptions> options,
    IHttpClientFactory clientFactory)
    : BaseBlobService(serviceProvider, blobServiceClientFactory, logger, options, clientFactory);

public abstract class BaseBlobService(
    IServiceProvider serviceProvider,
    IBlobServiceClientFactory blobServiceClientFactory,
    ILogger logger,
    IOptions<IBlobServiceOptions> options,
    IHttpClientFactory clientFactory)
    : AzureService(serviceProvider, logger, options.Value, clientFactory), IBlobService
{
    protected BlobContainerClient CreateBlobClient(string uri, string container, int timeout = default, int retries = default)
    {
        var blobServiceClient = blobServiceClientFactory.CreateBlobServiceClient(uri, timeout, retries);

        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        return containerClient;
    }

    private static string EnsurePrefixHasTrailingSlash(string prefix)
    {
        return !prefix.EndsWith('/') ? $"{prefix}/" : prefix;
    }

    public async Task<Status> CheckBlobAsync(string prefix = "RAW", int timeout = default, int retries = default)
    {
        return await CheckBlobAsync(options.Value.DmpBlobUri, options.Value.DmpBlobContainer, prefix, timeout, retries);
    }

    public async Task<Status> CheckBlobAsync(string uri, string container, string prefix = "RAW", int timeout = default, int retries = default)
    {
        prefix = EnsurePrefixHasTrailingSlash(prefix);

        Logger.LogInformation("Connecting to blob storage {Uri} : {BlobContainer}/{Prefix}. timeout={Timeout}, retries={Retries}",
            uri, container, prefix, timeout, retries);

        try
        {
            var containerClient = CreateBlobClient(uri, container, timeout, retries);

            Logger.LogInformation("Getting blob folders from {Prefix}...", prefix);
            var folders = containerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/");

            var itemCount = 0;
            await foreach (var blobItem in folders)
            {
                Logger.LogInformation("\t{Prefix}", blobItem.Prefix);
                itemCount++;
            }

            return new Status
            {
                Success = true,
                Description = $"Connected. {itemCount} blob folders found in {prefix}"
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading files");
            return new Status { Success = false, Description = ex.Message };
        }

    }

    [SuppressMessage("SonarLint", "S3267",
        Justification =
            "Ignored this is IAsyncEnumerable and doesn't support linq filtering out the box")]
    public async IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        prefix = EnsurePrefixHasTrailingSlash(prefix);

        Logger.LogDebug("Connecting to blob storage {BlobUri} : {BlobContainer} : {Path}", options.Value.DmpBlobUri,
            options.Value.DmpBlobContainer, prefix);

        var containerClient = CreateBlobClient(options.Value.DmpBlobUri, options.Value.DmpBlobContainer);

        var itemCount = 0;

        var files = containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken);


        await foreach (var item in files)
        {
            if (item.Properties.ContentLength is not 0 && item.Name.EndsWith(".json"))
            {
                yield return
                    new BtmsBlobItem { Name = item.Name, CreatedOn = item.Properties.CreatedOn };
                itemCount++;
            }
        }

        Logger.LogDebug("GetResourcesAsync {ItemCount} blobs found", itemCount);
    }

    public async Task<string> GetResource(IBlobItem item, CancellationToken cancellationToken)
    {
        var client = CreateBlobClient(options.Value.DmpBlobUri, options.Value.DmpBlobContainer, options.Value.Timeout, options.Value.Retries);
        var blobClient = client.GetBlobClient(item.Name);

        var content = await blobClient.DownloadContentAsync(cancellationToken);
        return content.Value.Content.ToString();
    }

    // If we want these, there's code in the old generator implementation
    // https://github.com/DEFRA/btms-backend/blob/b88e8354e26bf1ee1b9258647a00aea46d6970d6/TestDataGenerator/Services/BlobService.cs
    public Task<bool> CreateBlobsAsync(IBlobItem[] items)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CleanAsync(string prefix)
    {
        throw new NotImplementedException();
    }
}