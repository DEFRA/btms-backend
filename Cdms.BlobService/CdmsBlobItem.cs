using Azure.Storage.Blobs;

namespace Cdms.BlobService;

public class CdmsBlobItem : IBlobItem
{
    public string Name { get; set; } = default!;
    public string NormalisedName { get; set; } = default!;
    public string Content { get; set; } = default!;

}