using JsonApiDotNetCore.Resources.Annotations;

namespace Btms.Model.Relationships;

public class NotificationTdmRelationships : ITdmRelationships
{
    [Attr] public TdmRelationshipObject Movements { get; set; } = TdmRelationshipObject.CreateDefault();

    public List<(string, TdmRelationshipObject)> GetRelationshipObjects()
    {
        return [("movements", Movements)];
    }
}