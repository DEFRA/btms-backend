using System.Text.Json;
using System.Text.Json.Nodes;
using Cdms.Model.Extensions;
using Json.Patch;

namespace Cdms.Model.Auditing;

public class AuditEntry
{
    private const string CreatedBySystem = "System";
    public string Id { get; set; }
    public int Version { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? CreatedSource { get; set; }

    public DateTime CreatedLocal { get; set; } = System.DateTime.UtcNow;

    public string Status { get; set; }

    public List<AuditDiffEntry> Diff { get; set; } = new();


    public static AuditEntry Create<T>(T previous, T current, string id, int version, DateTime? lastUpdated,
        string lastUpdatedBy, string status)
    {
        var node1 = JsonNode.Parse(previous.ToJsonString());
        var node2 = JsonNode.Parse(current.ToJsonString());

        return CreateInternal(node1, node2, id, version, lastUpdated, status);
    }


    public static AuditEntry CreateUpdated<T>(T previous, T current, string id, int version, DateTime? lastUpdated)
    {
        return Create(previous, current, id, version, lastUpdated, CreatedBySystem, "Updated");
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

    public static AuditEntry CreateDecision(string previous, string current, string id, int version,
        DateTime? lastUpdated, string lastUpdatedBy)
    {
        var node1 = JsonNode.Parse(previous);
        var node2 = JsonNode.Parse(current);

        return CreateInternal(node1, node2, id, version, lastUpdated, "Decision");
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
            auditEntry.Diff.Add(AuditDiffEntry.CreateInternal(operation));
        }

        return auditEntry;
    }


    public class AuditDiffEntry
    {
        public string Path { get; set; }

        public string Op { get; set; }

        public object Value { get; set; }

        internal static AuditDiffEntry CreateInternal(PatchOperation operation)
        {
            object value = null;
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
                        value = operation.Value.GetValue<int>();
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