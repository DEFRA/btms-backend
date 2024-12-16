namespace Btms.BlobService;

public class BtmsBlobItem : IBlobItem
{
    public string Name { get; set; } = default!;
    public string NormalisedName { get; set; } = default!;
    public string Content { get; set; } = default!;

}