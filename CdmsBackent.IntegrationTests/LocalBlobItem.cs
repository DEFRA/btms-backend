using Cdms.BlobService;

namespace CdmsBackend.IntegrationTests;

public class LocalBlobItem(string name) : IBlobItem
{
    public string Name { get; set; } = name;

    public string NormalisedName { get; set; } = default;
    public string Content { get; set; } = default!;

    public async Task<string> Download(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            var content = await File.ReadAllTextAsync(Name, cancellationToken);
            Content = content;
        }

        return Content;
    }
}