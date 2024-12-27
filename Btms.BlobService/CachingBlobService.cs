using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Btms.BlobService;

public class CachingBlobService(
    [FromKeyedServices("base")] IBlobService blobService,
    ILogger<CachingBlobService> logger,
    IOptions<BlobServiceOptions> options
) : IBlobService
{
    public Task<Status> CheckBlobAsync(int timeout = default, int retries = default)
    {
        return blobService.CheckBlobAsync(timeout, retries);
    }

    public Task<Status> CheckBlobAsync(string uri, int timeout = default, int retries = default)
    {
        return blobService.CheckBlobAsync(uri, timeout, retries);
    }

    public async IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!options.Value.CacheReadEnabled)
        {
            await foreach (var blobItem in blobService.GetResourcesAsync(prefix, cancellationToken))
            {
                yield return blobItem;
            }
        }
        else
        {
            var cachePath = Path.GetFullPath(options.Value.CachePath);
            var path = Path.GetFullPath($"{options.Value.CachePath}/{prefix}");
            logger.LogInformation("Scanning disk {Path}", path);

            if (Directory.Exists(path))
            {
                logger.LogInformation("Folder {Path} exists, looking for files", path);  
                foreach (var f in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(cachePath, f);
                    logger.LogInformation("Found file {RelativePath}", relativePath);
                    yield return await Task.FromResult(new BtmsBlobItem { Name = relativePath });
                }           
            }
            else{
                logger.LogWarning("Cannot scan folder {Path} as it doesn't exist", path);    
            }
        }
    }

    public async Task<string> GetResource(IBlobItem item, CancellationToken cancellationToken)
    {
        if (!options.Value.CacheReadEnabled)
        {
            var blob =blobService.GetResource(item, cancellationToken);

            if (options.Value.CacheWriteEnabled)
            {
                item.Content = blob.Result;
                await CreateBlobAsync(item);
            }

            return blob.Result;
        }

        var filePath = $"{options.Value.CachePath}/{item.Name}";
        logger.LogInformation("GetResource {FilePath}", filePath);
        return File.ReadAllText(filePath);
    }
    
    public async Task<bool> CreateBlobsAsync(IBlobItem[] items)
    {
        foreach (var item in items) await CreateBlobAsync(item);

        return true;
    }
    
    private async Task CreateBlobAsync(IBlobItem item)
    {
        var fullPath = Path.GetFullPath($"{options.Value.CachePath}/{item.Name}");
        
        logger.LogInformation("Create folder for file {FullPath}", fullPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        logger.LogInformation("Create file {FullPath}", fullPath);
        await File.WriteAllTextAsync(fullPath, item.Content);
    }
    
    public Task<bool> CleanAsync(string prefix)
    {
        var fullPath = Path.GetFullPath($"{options.Value.CachePath}/{prefix}");
        logger.LogInformation("Clearing local storage {Path}", fullPath);
        
        try
        {
            Directory.Delete(fullPath, true);
            return Task.Run(() => true);
        }
        catch (DirectoryNotFoundException)
        {
            return Task.Run(() => true);
        }
    }
}
