using Btms.Model.Cds;
using MongoDB.Bson.Serialization.Attributes;

namespace Btms.Model.Auditing;

[BsonKnownTypes(typeof(DecisionContext), typeof(CdsFinalisation))]
public abstract class AuditContext
{
    
}