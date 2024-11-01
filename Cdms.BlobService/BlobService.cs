using System.ComponentModel.DataAnnotations;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Cdms.Azure;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Options;

namespace Cdms.BlobService;

public class BlobService(
    IBlobServiceClientFactory blobServiceClientFactory,
    ILogger<BlobService> logger,
    IOptions<BlobServiceOptions> options,
    IHttpClientFactory clientFactory)
    : AzureService<BlobService>(logger, options.Value, clientFactory), IBlobService
{
    private BlobContainerClient CreateBlobClient()
    {
        var blobServiceClient = blobServiceClientFactory.CreateBlobServiceClient();

        var containerClient = blobServiceClient.GetBlobContainerClient(options.Value.DmpBlobContainer);

        return containerClient;
    }


    public async IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Connecting to blob storage {BlobUri} : {BlobContainer}", options.Value.DmpBlobUri,
            options.Value.DmpBlobContainer);

        var containerClient = CreateBlobClient();

        Logger.LogInformation("Getting blob files from {Path}...", prefix);
        var itemCount = 0;

        var files = containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken);

        await foreach (BlobItem item in files)
        {
            if (item.Properties.ContentLength is not 0)
            {
                yield return
                    new SynchroniserBlobItem(containerClient.GetBlobClient(item.Name)) { Name = item.Name };
                itemCount++;
            }
        }

        Logger.LogInformation("GetResourcesAsync {itemCount} blobs found.", itemCount);
    }
}