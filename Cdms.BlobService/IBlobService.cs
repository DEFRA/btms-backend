namespace Cdms.BlobService;

public interface IBlobService
{
    public Task<Status> CheckBlobAsync();
    public Task<Status> CheckBlobAsync(string uri);
    public IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken);
}