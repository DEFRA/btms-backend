namespace Btms.BlobService;

public interface IBlobService
{
    public Task<Status> CheckBlobAsync(string prefix = "RAW", int timeout = default, int retries = default);
    public Task<Status> CheckBlobAsync(string uri, string container, string prefix = "RAW", int timeout = default, int retries = default);
    public IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken);
    public Task<string> GetResource(IBlobItem item, CancellationToken cancellationToken);
    public Task<bool> CreateBlobsAsync(IBlobItem[] items);
    public Task<bool> CleanAsync(string prefix);
}