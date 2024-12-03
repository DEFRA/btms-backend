using JsonApiDotNetCore.Resources.Annotations;

namespace Btms.Model.Relationships;

public sealed class TdmRelationshipObject
{
    [Attr] public bool? Matched { get; set; } = default!;

    [Attr] public RelationshipLinks Links { get; set; } = default!;

    [Attr] public List<RelationshipDataItem> Data { get; set; } = [];

    public static TdmRelationshipObject CreateDefault()
    {
        return new TdmRelationshipObject();
    }
}