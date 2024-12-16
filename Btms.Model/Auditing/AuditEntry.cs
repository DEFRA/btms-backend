using System.Text.Json;
using System.Text.Json.Nodes;
using Btms.Model.ChangeLog;
using Btms.Model.Extensions;
using Json.Patch;

namespace Btms.Model.Auditing;

public class AuditEntry
{
    private const string CreatedBySystem = "System";
    public string Id { get; set; } = default!;
    public int Version { get; set; }

    public string CreatedBy { get; set; } = default!;

    public DateTime? CreatedSource { get; set; }

    public DateTime CreatedLocal { get; set; } = System.DateTime.UtcNow;

    public string Status { get; set; } = default!;

    public List<AuditDiffEntry> Diff { get; set; } = new();

    public bool IsCreatedOrUpdated()
    {
        return IsCreated() || IsUpdated();
    }

    public bool IsCreated()
    {
        return this.Status == "Created";
    }

    public bool IsUpdated()
    {
        return this.Status == "Created";
    }


    public static AuditEntry Create<T>(T previous, T current, string id, int version, DateTime? lastUpdated,
        string lastUpdatedBy, string status)
    {
        var node1 = JsonNode.Parse(previous.ToJsonString());
        var node2 = JsonNode.Parse(current.ToJsonString());

        return CreateInternal(node1!, node2!, id, version, lastUpdated, status);
    }

    public static AuditEntry CreateUpdated<T>(T previous, T current, string id, int version, DateTime? lastUpdated)
    {
        return Create(previous, current, id, version, lastUpdated, CreatedBySystem, "Updated");
    }

    public static AuditEntry CreateUpdated(ChangeSet changeSet, string id, int version, DateTime? lastUpdated)
    {
        var auditEntry = new AuditEntry()
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = CreatedBySystem,
            CreatedLocal = DateTime.UtcNow,
            Status = "Updated"
        };

        foreach (var operation in changeSet.JsonPatch.Operations)
        {
            auditEntry.Diff.Add(AuditDiffEntry.Internal(operation));
        }
        
        return auditEntry;
    }

    public static AuditEntry CreateCreatedEntry<T>(T current, string id, int version, DateTime? lastUpdated)
    {
        return new AuditEntry()
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = CreatedBySystem,
            CreatedLocal = DateTime.UtcNow,
            Status = "Created"
        };
    }

    public static AuditEntry CreateSkippedVersion(string id, int version, DateTime? lastUpdated)
    {
        return new AuditEntry()
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = CreatedBySystem,
            CreatedLocal = DateTime.UtcNow,
            Status = "Updated"
        };
    }

    public static AuditEntry CreateLinked(string id, int version, DateTime? lastUpdated)
    {
        return new AuditEntry()
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = CreatedBySystem,
            CreatedLocal = DateTime.UtcNow,
            Status = "Linked"
        };
    }

    public static AuditEntry CreateMatch(string id, int version, DateTime? lastUpdated)
    {
        return new AuditEntry()
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = CreatedBySystem,
            CreatedLocal = DateTime.UtcNow,
            Status = "Matched"
        };
    }

    public static AuditEntry CreateDecision(string id, int version,
        DateTime? lastUpdated, string lastUpdatedBy)
    {
        return new AuditEntry()
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = CreatedBySystem,
            CreatedLocal = DateTime.UtcNow,
            Status = "Decision"
        };
    }

    private static AuditEntry CreateInternal(JsonNode previous, JsonNode current, string id, int version,
        DateTime? lastUpdated, string status)
    {
        var diff = previous.CreatePatch(current);

        var auditEntry = new AuditEntry()
        {
            Id = id,
            Version = version,
            CreatedSource = lastUpdated,
            CreatedBy = CreatedBySystem,
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

            return new AuditEntry.AuditDiffEntry()
            {
                Path = operation.Path.ToString(), Op = operation.Op.ToString(), Value = value
            };
        }
    }
}