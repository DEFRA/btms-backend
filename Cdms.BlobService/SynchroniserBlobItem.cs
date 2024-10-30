using Azure.Storage.Blobs;

namespace Cdms.BlobService;

public class SynchroniserBlobItem(BlobClient client) : IBlobItem
{
    public string Name { get; set; } = default!;

    public string NormalisedName { get; set; } = default;
    public string Content { get; set; } = default!;

    public async Task<string> Download(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            var content = await client.DownloadContentAsync(cancellationToken);
            Content = content.Value.Content.ToString();
        }

        return Content;
    }
}