namespace Cdms.BlobService;

public interface IBlobService
{
    public IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken);
}