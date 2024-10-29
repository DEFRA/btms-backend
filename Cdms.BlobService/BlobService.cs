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
    private BlobContainerClient CreateBlobClient(string serviceUri, int retries = 3, int timeout = 10)
    {
        var blobServiceClient = blobServiceClientFactory.CreateBlobServiceClient();

        var containerClient = blobServiceClient.GetBlobContainerClient(options.Value.DmpBlobContainer);

        return containerClient;
    }

    public async Task<Status> CheckBlobAsync()
    {
        return await CheckBlobAsync(options.Value.DmpBlobUri);
    }

    public async Task<Status> CheckBlobAsync(string serviceUri)
    {
        Logger.LogInformation("Connecting to blob storage {0} : {1}", serviceUri,
            options.Value.DmpBlobContainer);
        try
        {
            var containerClient = CreateBlobClient(serviceUri, 0, 5);

            Logger.LogInformation("Getting blob folders...");
            var folders = containerClient.GetBlobsByHierarchyAsync(prefix: "RAW/", delimiter: "/");

            var itemCount = 0;
            await foreach (BlobHierarchyItem blobItem in folders)
            {
                Logger.LogInformation("\t" + blobItem.Prefix);
                itemCount++;
            }

            return new Status()
            {
                Success = true, Description = String.Format("Connected. {0} blob folders found in RAW", itemCount)
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            return new Status() { Success = false, Description = ex.Message };
        }
    }

    public async IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix)
    {
        Logger.LogInformation("Connecting to blob storage {0} : {1}", options.Value.DmpBlobUri,
            options.Value.DmpBlobContainer);
        // try
        //{
        var containerClient = CreateBlobClient(options.Value.DmpBlobUri);

        Logger.LogInformation("Getting blob files from {0}...", prefix);
        var itemCount = 0;

        var files = containerClient.GetBlobsAsync(prefix: prefix);
        //var output = new List<IBlobItem>();

        await foreach (BlobItem item in files)
        {
            if (item.Properties.ContentLength is not 0)
            {
                Logger.LogInformation("\t" + item.Name);
                yield return
                    new SynchroniserBlobItem(containerClient.GetBlobClient(item.Name)) { Name = item.Name };
                itemCount++;
                //output.Add(new SynchroniserBlobItem(containerClient.GetBlobClient(item.Name)) { Name = item.Name });
            }
        }

        Logger.LogInformation($"GetResourcesAsync {itemCount} blobs found.");

        //return output;
        // }
        // catch (Exception ex)
        // {
        //  Logger.LogError(ex.ToString());
        //  throw;
        // }
    }

    public async Task<IBlobItem?> GetBlobAsync(string path)
    {
        Logger.LogInformation(
            $"Downloading blob {path} from blob storage {options.Value.DmpBlobUri} : {options.Value.DmpBlobContainer}");
        try
        {
            var containerClient = CreateBlobClient(options.Value.DmpBlobUri);

            var blobClient = containerClient.GetBlobClient(path);

            var content = await blobClient.DownloadContentAsync();

            // content.Value.Content.
            return new SynchroniserBlobItem(blobClient) { Name = path, Content = content.Value.Content.ToString()! };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    }
}