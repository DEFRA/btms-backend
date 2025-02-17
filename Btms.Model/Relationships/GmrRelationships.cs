using JsonApiDotNetCore.Resources.Annotations;

namespace Btms.Model.Relationships;

public class GmrRelationships : ITdmRelationships
{
    [Attr] public TdmRelationshipObject ImportNotifications { get; set; } = TdmRelationshipObject.CreateDefault();

    public List<(string, TdmRelationshipObject)> GetRelationshipObjects()
    {
        return [("import-notifications", ImportNotifications)];
    }
}