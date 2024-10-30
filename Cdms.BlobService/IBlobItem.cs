namespace Cdms.BlobService;

public interface IBlobItem
{
    string Name { get; set; }
    string Content { get; set; }

    Task<string> Download(CancellationToken cancellationToken);
}