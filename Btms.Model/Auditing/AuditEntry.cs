using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Btms.Model.Cds;
using Btms.Model.ChangeLog;
using Btms.Model.Extensions;
using Json.Patch;
using MongoDB.Bson.Serialization.Attributes;

namespace Btms.Model.Auditing;

[JsonConverter(typeof(JsonStringEnumConverterEx<CreatedBySystem>))]
public enum CreatedBySystem
{
    Btms,
    Ipaffs,
    Alvs,
    Cds,
    Gvms
}
public class AuditEntry
{
    public string Id { get; set; } = default!;
    public int? Version { get; set; }

    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public CreatedBySystem CreatedBy { get; set; } = default!;

    public DateTime? CreatedSource { get; set; }

    public DateTime CreatedLocal { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = default!;

    public List<AuditDiffEntry> Diff { get; set; } = new();
    
    // TODO - getting a serialisation error when using IAuditContext
    // But as we only do this for decisions ignoring! 
    public DecisionContext? Context { get; set; }

    public bool IsCreatedOrUpdated()
    {
        return IsCreated() || IsUpdated();
    }

    public bool IsCreated()
    {
        return Status == "Created";
    }

    public bool IsUpdated()
    {
        return Status == "Updated";
    }

    public bool IsDeleted()
    {
        return Status == "Deleted";
    }


    public static AuditEntry Create<T>(T previous, T current, string id, int version, DateTime? lastUpdated,
        CreatedBySystem lastUpdatedBy, string status, CreatedBySystem source)
    {
        var node1 = JsonNode.Parse(previous.ToJsonString());
        var node2 = JsonNode.Parse(current.ToJsonString());

        return CreateInternal(node1!, node2!, id, version, lastUpdated, status, source);
    }

    public static AuditEntry CreateUpdated<T>(T previous, T current, string id, int version, DateTime? lastUpdated, CreatedBySystem source)
    {
        return Create(previous, current, id, version, lastUpdated, CreatedBySystem.Btms, "Updated", source);
    }

    public static AuditEntry CreateUpdated(ChangeSet changeSet, string id, int version, DateTime? lastUpdated, CreatedBySystem source)
    {
        return CreateUpdated(changeSet, id, version, lastUpdated, source, "Updated");
    }

    public static AuditEntry CreateCancelled(ChangeSet changeSet, string id, int version, DateTime? lastUpdated, CreatedBySystem source)
    {
        return CreateUpdated(changeSet, id, version, lastUpdated, source, "Cancelled");
    }

    public static AuditEntry CreateDeleted(ChangeSet changeSet, string id, int version, DateTime? lastUpdated, CreatedBySystem source)
    {
        return CreateUpdated(changeSet, id, version, lastUpdated, source, "Deleted");
    }

    private static AuditEntry CreateUpdated(ChangeSet changeSet, string id, int version, DateTime? lastUpdated, CreatedBySystem source, string status)
    {
        var auditEntry = new AuditEntry
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = source,
            CreatedLocal = DateTime.UtcNow,
            Status = status
        };

        foreach (var operation in changeSet.JsonPatch.Operations)
        {
            auditEntry.Diff.Add(AuditDiffEntry.Internal(operation));
        }

        return auditEntry;
    }

    public static AuditEntry CreateCreatedEntry<T>(T current, string id, int version, DateTime? lastUpdated, CreatedBySystem source)
    {
        return new AuditEntry
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = source,
            CreatedLocal = DateTime.UtcNow,
            Status = "Created"
        };
    }

    public static AuditEntry CreateSkippedVersion(string id, int version, DateTime? lastUpdated, CreatedBySystem source)
    {
        return new AuditEntry
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = source,
            CreatedLocal = DateTime.UtcNow,
            Status = "Updated"
        };
    }

    public static AuditEntry CreateLinked(string id, int version)
    {
        var t = DateTime.UtcNow;
        return new AuditEntry
        {
            Id = id,
            CreatedSource = t,
            CreatedBy = CreatedBySystem.Btms,
            CreatedLocal = t,
            Status = "Linked"
        };
    }
    
    public static AuditEntry CreateUnlinked(string id, int version, DateTime? lastUpdated)
    {
        return new AuditEntry
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = CreatedBySystem.Btms,
            CreatedLocal = DateTime.UtcNow,
            Status = "Unlinked"
        };
    }

    public static AuditEntry CreateMatch(string id, int version)
    {
        var t = DateTime.UtcNow;
        return new AuditEntry
        {
            Id = id,
            CreatedSource = t,
            CreatedBy = CreatedBySystem.Btms,
            CreatedLocal = t,
            Status = "Matched"
        };
    }

    public static AuditEntry CreateDecision(string id, int version,
        DateTime? lastUpdated, string lastUpdatedBy, DecisionContext context, bool isAlvs)
    {
        return new AuditEntry()
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = isAlvs ? CreatedBySystem.Alvs : CreatedBySystem.Btms,
            CreatedLocal = DateTime.UtcNow,
            Status = "Decision",
            Context = context
        };
    }

    private static AuditEntry CreateInternal(JsonNode previous, JsonNode current, string id, int version,
        DateTime? lastUpdated, string status, CreatedBySystem source)
    {
        var diff = previous.CreatePatch(current);

        var auditEntry = new AuditEntry
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = source,
            CreatedLocal = DateTime.UtcNow,
            Status = status
        };

        foreach (var operation in diff.Operations)
        {
            auditEntry.Diff.Add(AuditDiffEntry.Internal(operation));
        }

        return auditEntry;
    }


    public class AuditDiffEntry
    {
        public string Path { get; set; } = null!;

        public string Op { get; set; } = null!;

        public object Value { get; set; } = null!;

        internal static AuditDiffEntry Internal(PatchOperation operation)
        {
            object value = null!;
            if (operation.Value != null)
            {
                switch (operation.Value.GetValueKind())
                {
                    case JsonValueKind.Undefined:
                        value = "UNKNOWN";
                        break;
                    case JsonValueKind.Object:
                        value = "COMPLEXTYPE";
                        break;
                    case JsonValueKind.Array:
                        value = "ARRAY";
                        break;
                    case JsonValueKind.String:
                        value = operation.Value.GetValue<string>();
                        break;
                    case JsonValueKind.Number:
                        value = operation.Value.GetValue<double>();
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        value = operation.Value.GetValue<bool>();
                        break;
                    case JsonValueKind.Null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"Unhandled JsonValueKind {operation.Value.GetValueKind()}");
                }
            }

            return new AuditDiffEntry
            {
                Path = operation.Path.ToString(), Op = operation.Op.ToString(), Value = value
            };
        }
    }
}