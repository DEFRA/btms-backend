using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;
using Btms.Model.Cds;
using MongoDB.Bson.Serialization.Attributes;

namespace Btms.Model.Auditing;

[BsonKnownTypes(typeof(DecisionContext), typeof(CdsFinalisation))]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(DecisionContext), nameof(DecisionContext))]
[JsonDerivedType(typeof(CdsFinalisation), nameof(CdsFinalisation))]
public abstract class AuditContext
{

}