namespace Btms.BlobService;

public interface IBlobService
{
    public Task<Status> CheckBlobAsync(int timeout = default, int retries = default);
    public Task<Status> CheckBlobAsync(string uri, int timeout = default, int retries = default);
    public IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken);
    public Task<string> GetResource(IBlobItem item, CancellationToken cancellationToken);
    public Task<bool> CreateBlobsAsync(IBlobItem[] items);
    public Task<bool> CleanAsync(string prefix);
}