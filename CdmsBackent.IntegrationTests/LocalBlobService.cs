using Cdms.BlobService;

namespace CdmsBackend.IntegrationTests;

public class LocalBlobService(string root) : IBlobService
{
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