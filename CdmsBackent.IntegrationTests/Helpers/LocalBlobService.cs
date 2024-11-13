using Cdms.BlobService;

namespace CdmsBackend.IntegrationTests.Helpers;

public class LocalBlobService(string root) : IBlobService
{
    public Task<Status> CheckBlobAsync(int timeout = default, int retries = default)
    {
        throw new NotImplementedException();
    }

    public Task<Status> CheckBlobAsync(string uri, int timeout = default, int retries = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix, CancellationToken cancellationToken)
    {
        return ScanFiles(Path.Combine(root, prefix), cancellationToken);
    }

    public async IAsyncEnumerable<IBlobItem> ScanFiles(string prefix, CancellationToken cancellationToken)
    {
        foreach (string f in Directory.GetFiles(prefix))
        {
            yield return new LocalBlobItem(f);
        }

        foreach (string d in Directory.GetDirectories(prefix))
        {
            await foreach (var item in GetResourcesAsync(d, cancellationToken))
            {
                yield return item;
            }
        }
    }
}