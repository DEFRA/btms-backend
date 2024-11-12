using JsonApiDotNetCore.Resources.Annotations;

namespace Cdms.Model.Relationships;

public sealed class TdmRelationshipObject
{
    [Attr] public bool? Matched { get; set; } = default!;

    [Attr] public RelationshipLinks Links { get; set; }

    [Attr] public List<RelationshipDataItem> Data { get; set; } = new List<RelationshipDataItem>();

    public static TdmRelationshipObject CreateDefault()
    {
        return new TdmRelationshipObject();
    }
}