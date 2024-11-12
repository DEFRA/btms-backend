namespace Cdms.Model.Relationships;

public interface ITdmRelationships
{
    public List<(string, TdmRelationshipObject)> GetRelationshipObjects();
}