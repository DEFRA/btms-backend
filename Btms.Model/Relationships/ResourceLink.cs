using JsonApiDotNetCore.Resources.Annotations;

namespace Btms.Model.Relationships;

public sealed class ResourceLink
{
    [Attr]
    public string Self { get; set; } = default!;
}