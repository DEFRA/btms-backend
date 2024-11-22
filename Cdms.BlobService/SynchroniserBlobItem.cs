using Azure.Storage.Blobs;

namespace Cdms.BlobService;

public class SynchroniserBlobItem : IBlobItem
{
    public string Name { get; set; } = default!;
    public string Content { get; set; } = default!;

    public async Task<string> Download(BlobClient client, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            var content = await client.DownloadContentAsync(cancellationToken);
            Content = content.Value.Content.ToString();
        }

        return Content;
    }
}