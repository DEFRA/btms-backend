namespace Cdms.BlobService;

public interface IBlobService
{
    public Task<Status> CheckBlobAsync(int timeout = default, int retries = default);
    public Task<Status> CheckBlobAsync(string uri, int timeout = default, int retries = default);
    public IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken);
    public Task<string> GetResource(IBlobItem item, CancellationToken cancellationToken);
}