using System.Diagnostics.Tracing;
using Azure.Core;
using Azure.Core.Diagnostics;
using Azure.Core.Pipeline;
using Azure.Identity;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

using TestDataGenerator.Config;

namespace TestDataGenerator.Services;

//I've copied this all over from TDM initially, imagine we'll end up with an Azure lib in the Cdms shortly
public interface IAzureConfig
{
    public string? AzureClientId { get; }
    public string? AzureTenantId { get; }
    public string? AzureClientSecret { get; }
}

public class Status
{
    public string Description { get; set; } = default!;

    public bool Success { get; set; }
}

public interface IBlobItem
{
    string Name { get; set; }
    string Content { get; set; }
    
}

public class BlobItem : IBlobItem
{
    public string Name { get; set; } = default!;

    public string NormalisedName { get; set; } = default;
    
    public string Content { get; set; } = default!;
}

public interface IBlobService
{
    public Task<bool> CleanAsync(string prefix);
    public Task<bool> CreateBlobsAsync(IBlobItem[] items);
}

public abstract class AzureService<T> 
{
    protected readonly TokenCredential Credentials;
    protected readonly HttpClientTransport? Transport;
    protected readonly ILogger<T> Logger;
    
    protected AzureService(ILogger<T> logger, IAzureConfig config, IHttpClientFactory? clientFactory = null)
    {
        Logger = logger;
        using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);

        if (config.AzureClientId != null)
        {
            logger.LogInformation($"Creating azure credentials based on config vars for {config.AzureClientId}");
            Credentials =
                new ClientSecretCredential(config.AzureTenantId, config.AzureClientId, config.AzureClientSecret);

            logger.LogInformation($"Created azure credentials");
        }
        else
        {
            logger.LogInformation($"Creating azure credentials using default creds because AZURE_CLIENT_ID env var not found.");
            Credentials = new DefaultAzureCredential();
            logger.LogInformation($"Created azure default credentials");
        }

        if (clientFactory != null)
        {
            Transport = new HttpClientTransport(clientFactory.CreateClient("proxy"));    
        }
        
    }
}

public class LocalBlobService(ILogger<LocalBlobService> logger) : IBlobService
{
    private string _rootPath = "../../../.test-data-generator/";
        
    public Task<bool> CleanAsync(string prefix)
    {
        try
        {
            logger.LogInformation("Clearing local storage");
            Directory.Delete($"{_rootPath}{prefix}", true);
            return Task.Run(() =>  true);
        }
        catch (DirectoryNotFoundException)
        {
            return Task.Run(() =>  true);
        }
    }

    public async Task<bool> CreateBlobAsync(IBlobItem item)
    {
        var fullPath = $"{_rootPath}{item.Name}";
        
        logger.LogInformation($"Create folder for file {fullPath}");
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        
        logger.LogInformation($"Create file {fullPath}");
        await File.WriteAllTextAsync(fullPath, item.Content);
        
        return true;
    }

    public async Task<bool> CreateBlobsAsync(IBlobItem[] items)
    {
        foreach (var item in items)
        {
            await CreateBlobAsync(item);
        }

        return true;
    }
}

public class BlobService(ILogger<BlobService> logger, GeneratorConfig config, IHttpClientFactory clientFactory)
    : AzureService<BlobService>(logger, config, clientFactory), IBlobService
{
    private BlobContainerClient CreateBlobClient(string serviceUri, int retries = 3, int timeout = 2)
    {
        var options = new BlobClientOptions
        {
            Transport = Transport!,
            Retry =
            {
                MaxRetries = retries,
                NetworkTimeout = TimeSpan.FromSeconds(timeout)
            },
            
            Diagnostics = 
            {
                IsLoggingContentEnabled = true,
                IsLoggingEnabled = true
            }
        };
        
        var blobServiceClient = new BlobServiceClient(
            new Uri(serviceUri),
            Credentials,
            options);

        var containerClient = blobServiceClient.GetBlobContainerClient(config.DmpBlobContainer);
        
        return containerClient;
    }

    public async Task<bool> CleanAsync(string prefix)
    {
        Logger.LogInformation($"Cleaning blob storage {config.DmpBlobUri} : {config.DmpBlobContainer} / ${prefix}");
        
        try
        {
            var containerClient = CreateBlobClient(config.DmpBlobUri);
            var allBlobs = await GetResourcesAsync(prefix);
            
            foreach (var blobItem in allBlobs)
            {
                Logger.LogInformation($"Deleting {blobItem.Name}");
                await containerClient.DeleteBlobAsync(blobItem.Name);
            }
            
            // TODO : we currently delete the files, but not the folders, not the end of the world but would prefer to
            // Remove the folders too - it means doing it recursively from the leaf as you can't remove a folder that has folders
            // or files in it :|
            
            // var directories = containerClient.GetBlobsByHierarchyAsync(prefix: prefix, delimiter:"/");
            //
            // await foreach (var blobItem in directories)
            // {
            //     Logger.LogInformation($"Found {blobItem.Prefix}, item IsBlob {blobItem.IsBlob}, IsPrefix {blobItem.IsPrefix}");
            //     await containerClient.DeleteBlobAsync(blobItem.Prefix);
            // }
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            return false;
        }
    }

    public async Task<bool> CreateBlobAsync(IBlobItem item)
    {
        Logger.LogInformation("Connecting to blob storage {0} : {1}", config.DmpBlobUri,
            config.DmpBlobContainer);
        try
        {
            var containerClient = CreateBlobClient(config.DmpBlobUri);
            var result = await containerClient.UploadBlobAsync(item.Name, BinaryData.FromString(item.Content));
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            return false;
        }
    }
    
    public async Task<bool> CreateBlobsAsync(IBlobItem[] items)
    {
        Logger.LogInformation("Connecting to blob storage {0} : {1}", config.DmpBlobUri,
            config.DmpBlobContainer);
        try
        {
            var containerClient = CreateBlobClient(config.DmpBlobUri);
            
            foreach (var item in items)
            {
                try
                {
                    Logger.LogInformation($"Uploading file {item.Name}");
                    var result = await containerClient.UploadBlobAsync(item.Name, BinaryData.FromString(item.Content));
                    Logger.LogInformation($"Uploaded file {item.Name} Result = {result}");
                    // if (result.Value.)
                    
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error uploading file ${item.Name}, {ex.ToString()}");
                    throw;
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error uploading files ${ex.ToString()}");
            return false;
        }
    }

    // public async Task<Status> CheckBlobAsync()
    // {
    //     return await CheckBlobAsync(config.DmpBlobUri);
    // }
    //
    // public async Task<Status> CheckBlobAsync(string serviceUri)
    // {
    //     Logger<>.LogInformation("Connecting to blob storage {0} : {1}", serviceUri,
    //         config.DmpBlobContainer);
    //     try
    //     {
    //         var containerClient = CreateBlobClient(serviceUri, 0, 5);
    //         
    //         Logger<>.LogInformation("Getting blob folders...");
    //         var folders = containerClient.GetBlobsByHierarchyAsync(prefix: "RAW/", delimiter: "/");
    //
    //         var itemCount = 0;
    //         await foreach (BlobHierarchyItem blobItem in folders)
    //         {
    //             Logger<>.LogInformation("\t" + blobItem.Prefix);
    //             itemCount++;
    //         }
    //
    //         return new Status()
    //         {
    //             Success = true, Description = String.Format("Connected. {0} blob folders found in RAW", itemCount)
    //         };
    //     }
    //     catch (Exception ex)
    //     {
    //         Logger<>.LogError(ex.ToString());
    //         return new Status() { Success = false, Description = ex.Message };
    //     }
    //
    // }
    //
    public async Task<IEnumerable<IBlobItem>> GetResourcesAsync(string prefix)
    {
        Logger.LogInformation("Connecting to blob storage {0} : {1}", config.DmpBlobUri,
            config.DmpBlobContainer);
        try
        {
            var containerClient = CreateBlobClient(config.DmpBlobUri);
    
            Logger.LogInformation("Getting blob files from {0}...", prefix);
            // var itemCount = 0;
            
            var files = containerClient.GetBlobsAsync(prefix: prefix);
            var output = new List<IBlobItem>();
            
            await foreach (var item in files)
            {
                if (item.Properties.ContentLength is not 0)
                {
                    Logger.LogInformation("\t" + item.Name);
                    // itemCount++;
                    output.Add(new BlobItem() { Name = item.Name });
                }
            }
            
            Logger.LogInformation($"GetResourcesAsync {output.Count} blobs found.");
    
            return output;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            throw;
        }
    
    }
    //
    // public async Task<IBlobItem?> GetBlobAsync(string path)
    // {
    //     Logger<>.LogInformation(
    //         $"Downloading blob {path} from blob storage {config.DmpBlobUri} : {config.DmpBlobContainer}");
    //     try
    //     {
    //         var containerClient = CreateBlobClient(config.DmpBlobUri);
    //
    //         var blobClient = containerClient.GetBlobClient(path);
    //
    //         var content = await blobClient.DownloadContentAsync();
    //         
    //         // content.Value.Content.
    //         return new SynchroniserBlobItem() { Name = path, Content = content.Value.Content.ToString()! };
    //     }
    //     catch (Exception ex)
    //     {
    //         Logger<>.LogError(ex.ToString());
    //         throw;
    //     }
    //
    // }
}