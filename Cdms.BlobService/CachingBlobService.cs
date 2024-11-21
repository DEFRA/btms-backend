using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cdms.BlobService;

public class CachingBlobService(
    [FromKeyedServices("base")] IBlobService blobService,
    ILogger<CachingBlobService> logger,
    IOptions<BlobServiceOptions> options
) : IBlobService
{
    public Task<Status> CheckBlobAsync(int timeout = default, int retries = default)
    {
        return blobService.CheckBlobAsync();
    }

    public Task<Status> CheckBlobAsync(string uri, int timeout = default, int retries = default)
    {
        return blobService.CheckBlobAsync(uri, timeout, retries);
    }

    public async IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken)
    {
        var path = Path.GetFullPath($"{options.Value.CachePath}/{prefix}");
        logger.LogInformation("Scanning disk {path}", path);

        if (Directory.Exists(path))
        {
            logger.LogInformation("Folder {path} exists, looking for files.", path);  
           foreach (string f in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
           {
               var relativePath = Path.GetRelativePath($"{Directory.GetCurrentDirectory()}/{options.Value.CachePath}", f);
                logger.LogInformation("Found file {relativePath}", relativePath);
                yield return new SynchroniserBlobItem() { Name = relativePath };
            }           
        }
        else{
            logger.LogWarning("Cannot scan folder {path} as it doesn't exist.", path);    
        }
    }

    public Task<string> GetResource(IBlobItem item, CancellationToken cancellationToken)
    {
        var filePath = $"{options.Value.CachePath}/{item.Name}";
        logger.LogInformation("GetResource {filePath}", filePath);
        return Task.Run(() => File.ReadAllText(filePath));
    }
}
