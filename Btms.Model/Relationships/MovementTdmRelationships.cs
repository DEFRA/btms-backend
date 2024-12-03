using JsonApiDotNetCore.Resources.Annotations;

namespace Btms.Model.Relationships;

public class MovementTdmRelationships : ITdmRelationships
{
    [Attr] public TdmRelationshipObject Notifications { get; set; } = TdmRelationshipObject.CreateDefault();

    public List<(string, TdmRelationshipObject)> GetRelationshipObjects()
    {
        return [("notifications", Notifications)];
    }
}