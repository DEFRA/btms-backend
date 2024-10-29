using Azure.Storage.Blobs;

namespace Cdms.BlobService;

public class SynchroniserBlobItem(BlobClient client) : IBlobItem
{
    public string Name { get; set; } = default!;

    public string NormalisedName { get; set; } = default;
    public string Content { get; set; } = default!;

    public async Task<string> Download()
    {
        var content = await client.DownloadContentAsync();
        Content = content.Value.Content.ToString();
        return Content;
    }
}