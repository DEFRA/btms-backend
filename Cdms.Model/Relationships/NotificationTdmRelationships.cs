using JsonApiDotNetCore.Resources.Annotations;

namespace Cdms.Model.Relationships;

public class NotificationTdmRelationships : ITdmRelationships
{
    [Attr] public TdmRelationshipObject Movements { get; set; } = TdmRelationshipObject.CreateDefault();

    public (string, TdmRelationshipObject) GetRelationshipObject()
    {
        return ("movements", Movements);
    }
}