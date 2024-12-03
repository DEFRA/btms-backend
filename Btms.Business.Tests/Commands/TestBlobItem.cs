using Btms.BlobService;

namespace Btms.Business.Tests.Commands;

public class TestBlobItem(string name, string content) : IBlobItem
{
    public string Name { get; set; } = name!;
    
    public string Content { get; set; } = content!;
}