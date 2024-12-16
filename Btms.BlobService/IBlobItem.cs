namespace Btms.BlobService;

public interface IBlobItem
{
    string Name { get; set; }
    string Content { get; set; }
}