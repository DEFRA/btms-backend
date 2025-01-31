using System.Diagnostics;

namespace Btms.BlobService;

[DebuggerDisplay("{CreatedOn} - {Name}")]
public class BtmsBlobItem : IBlobItem
{
    public string Name { get; set; } = default!;
    public string NormalisedName { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTimeOffset? CreatedOn { get; set; }

}