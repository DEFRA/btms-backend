namespace Btms.Model.Relationships;

public interface ITdmRelationships
{
    public List<(string, TdmRelationshipObject)> GetRelationshipObjects();
}