using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Cdms.Model.Alvs;
using Cdms.Model.Auditing;
using Cdms.Model.Data;
using Cdms.Model.Extensions;
using Cdms.Model.Relationships;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Cdms.Model;

// Recreation of ClearanceRequest schema from
// https://eaflood.atlassian.net/wiki/spaces/TRADE/pages/5104664583/PHA+Port+Health+Authority+Integration+Data+Schema

[Resource]
public class Movement : IMongoIdentifiable, IDataEntity
{
    private List<string> matchReferences = [];

    // This field is used by the jsonapi-consumer to control the correct casing in the type field
    public string Type { get; set; } = "movements";

    [Attr] public List<Alvs.AlvsClearanceRequest> ClearanceRequests { get; set; } = default!;

    [Attr] public List<Alvs.AlvsClearanceRequest> Decisions { get; set; } = default!;

    [Attr] public List<Items> Items { get; set; } = [];

    /// <summary>
    /// Date when the notification was last updated.
    /// </summary>
    [Attr]
    public DateTime? LastUpdated { get; set; }

    [Attr] public string EntryReference { get; set; } = default!;

    [Attr] public string MasterUcr { get; set; } = default!;

    [Attr] public int? DeclarationPartNumber { get; set; }

    [Attr] public string DeclarationType { get; set; } = default!;

    [Attr] public DateTime? ArrivedOn { get; set; }

    [Attr] public string SubmitterTurn { get; set; } = default!;

    [Attr] public string DeclarantId { get; set; } = default!;

    [Attr] public string DeclarantName { get; set; } = default!;

    [Attr] public string DispatchCountryCode { get; set; } = default!;

    [Attr] public string GoodsLocationCode { get; set; } = default!;

    [Attr] public List<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();

    [Attr]
    [JsonPropertyName("relationships")]
    public MovementTdmRelationships Relationships { get; set; } = new MovementTdmRelationships();

    /// <summary>
    /// Tracks the last time the record was changed
    /// </summary>
    [Attr]
    [BsonElement("_ts")]
    public DateTime _Ts { get; set; }

    [BsonElement("_matchReferences")]
    public List<string> _MatchReferences
    {
        get
        {
            if (matchReferences.Any())
            {
                var list = new HashSet<string>();
                foreach (var item in Items.Where(x => x.Documents != null))
                {
                    foreach (var itemDocument in item.Documents!)
                    {
                        list.Add(MatchIdentifier.FromCds(itemDocument.DocumentReference!).Identifier);
                    }
                }

                matchReferences = list.ToList();
            }

            return matchReferences;
        }
        set => matchReferences = value;
    }

    public void AddRelationship(string type, TdmRelationshipObject relationship)
    {
        Relationships.Notifications.Links ??= relationship.Links;
        foreach (var dataItem in relationship.Data.Where(dataItem => Relationships.Notifications.Data.TrueForAll(x => x.Id != dataItem.Id)))
        {
            Relationships.Notifications.Data.Add(dataItem);
        }

        Relationships.Notifications.Matched = Items
            .Select(x => x.ItemNumber)
            .All(itemNumber =>
                Relationships.Notifications.Data.Exists(x => x.Matched.GetValueOrDefault() && x.SourceItem == itemNumber));
    }

    public void Update(AuditEntry auditEntry)
    {
        this.AuditEntries.Add(auditEntry);
        _Ts = DateTime.UtcNow;
        matchReferences = [];
    }

    public bool MergeDecision(string path, AlvsClearanceRequest clearanceRequest)
    {
        var before = this.ToJsonString();
        foreach (var item in clearanceRequest.Items!)
        {
            var existingItem = this.Items.Find(x => x.ItemNumber == item.ItemNumber);

            if (existingItem is not null)
            {
                existingItem.MergeChecks(item);
            }
        }

        var after = this.ToJsonString();

        var auditEntry = AuditEntry.CreateDecision(before, after,
            BuildNormalizedDecisionPath(path),
            clearanceRequest.Header!.EntryVersionNumber.GetValueOrDefault(),
            clearanceRequest.ServiceHeader!.ServiceCalled,
            clearanceRequest.Header.DeclarantName!);
        if (auditEntry.Diff.Any())
        {
            Decisions ??= new List<AlvsClearanceRequest>();
            Decisions.Add(clearanceRequest);
            this.Update(auditEntry);
        }

        return auditEntry.Diff.Any();
    }

    private static string BuildNormalizedDecisionPath(string fullPath)
    {
        return fullPath.Replace("RAW/DECISIONS/", "");
    }

    [BsonIgnore]
    [NotMapped]
    public string? StringId
    {
        get => Id;
        set => Id = value;
    }

    [BsonIgnore]
    [NotMapped]
    [Attr]
    public string? LocalId { get; set; }

    [Attr]
    [BsonId]
    public string? Id { get; set; } = null!;

    public string _Etag { get; set; } = null!;
}