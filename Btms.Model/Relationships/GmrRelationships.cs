using JsonApiDotNetCore.Resources.Annotations;

namespace Btms.Model.Relationships;

public class GmrRelationships : ITdmRelationships
{
    [Attr] public TdmRelationshipObject Transits { get; set; } = TdmRelationshipObject.CreateDefault();

    [Attr] public TdmRelationshipObject Customs { get; set; } = TdmRelationshipObject.CreateDefault();

    public List<(string, TdmRelationshipObject)> GetRelationshipObjects()
    {
        return [("transits", Transits), ("customs", Customs)];
    }
}