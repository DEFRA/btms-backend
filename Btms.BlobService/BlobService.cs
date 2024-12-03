using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Btms.Azure;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace Btms.BlobService;

public class BlobService(
    IBlobServiceClientFactory blobServiceClientFactory,
    ILogger<BlobService> logger,
    IOptions<BlobServiceOptions> options,
    IHttpClientFactory clientFactory)
    : AzureService(logger, options.Value, clientFactory), IBlobService
{
    private BlobContainerClient CreateBlobClient(int timeout = default, int retries = default)
    {
        var blobServiceClient = blobServiceClientFactory.CreateBlobServiceClient(timeout, retries);

        var containerClient = blobServiceClient.GetBlobContainerClient(options.Value.DmpBlobContainer);

        return containerClient;
    }
    public async Task<Status> CheckBlobAsync(int timeout = default, int retries = default)
    {
        return await CheckBlobAsync(options.Value.DmpBlobUri, timeout, retries);
    }
    
    public async Task<Status> CheckBlobAsync(string uri, int timeout = default, int retries = default)
    {
        Logger.LogInformation("Connecting to blob storage {Uri} : {BlobContainer}. timeout={Timeout}, retries={Retries}.",
            uri, options.Value.DmpBlobContainer, timeout, retries);
        
        try
        {
            var containerClient = CreateBlobClient(timeout, retries);
            
            Logger.LogInformation("Getting blob folders...");
            var folders = containerClient.GetBlobsByHierarchyAsync(prefix: "RAW/", delimiter: "/");

            var itemCount = 0;
            await foreach (BlobHierarchyItem blobItem in folders)
            {
                Logger.LogInformation("\t{Prefix}", blobItem.Prefix);
                itemCount++;
            }

            return new Status()
            {
                Success = true, Description = $"Connected. {itemCount} blob folders found in RAW"
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading files");
            return new Status() { Success = false, Description = ex.Message };
        }

    }

    [SuppressMessage("SonarLint", "S3267",
        Justification =
            "Ignored this is IAsyncEnumerable and doesn't support linq filtering out the box")]
    public async IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Logger.LogDebug("Connecting to blob storage {BlobUri} : {BlobContainer} : {Path}", options.Value.DmpBlobUri,
            options.Value.DmpBlobContainer, prefix);

        var containerClient = CreateBlobClient();

        var itemCount = 0;

        var files = containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken);


        await foreach (BlobItem item in files)
        {
            if (item.Properties.ContentLength is not 0)
            {
                yield return
                    new BtmsBlobItem() { Name = item.Name };
                itemCount++;
            }
        }

        Logger.LogDebug("GetResourcesAsync {ItemCount} blobs found.", itemCount);
    }

    public async Task<string> GetResource(IBlobItem item, CancellationToken cancellationToken)
    {
        var client = CreateBlobClient(options.Value.Timeout, options.Value.Retries);
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