using System.Text.Json.Serialization;
using Cdms.Model.Alvs;
using Cdms.Model.Auditing;
using Cdms.Model.Data;
using Cdms.Model.Extensions;
using Cdms.Model.Relationships;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

// using JsonApiSerializer.JsonApi;

namespace Cdms.Model;
// namespace TdmPrototypeBackend.Models;

// Recreation of ClearanceRequest schema from
// https://eaflood.atlassian.net/wiki/spaces/TRADE/pages/5104664583/PHA+Port+Health+Authority+Integration+Data+Schema

[Resource]
public class Movement : IMongoIdentifiable, IDataEntity
{
    private List<int> matchReferences;

    // This field is used by the jsonapi-consumer to control the correct casing in the type field
    public string Type { get; set; } = "movements";

    //[Attr]
    //public List<MatchingStatus> Notifications { get; set; } = [new() { Matched = false }];

    [Attr] public List<Alvs.AlvsClearanceRequest> ClearanceRequests { get; set; } = default!;

    [Attr] public List<Alvs.AlvsClearanceRequest> Decisions { get; set; } = default!;

    [Attr] public List<Items> Items { get; set; } = default!;

    /// <summary>
    /// Date when the notification was last updated.
    /// </summary>
    [Attr]
    public DateTime? LastUpdated { get; set; }

    [Attr] public string EntryReference { get; set; }

    [Attr] public string MasterUCR { get; set; }

    [Attr] public int? DeclarationPartNumber { get; set; }

    [Attr] public string DeclarationType { get; set; }

    [Attr] public DateTime? ArrivalDateTime { get; set; }

    [Attr] public string SubmitterTURN { get; set; }

    [Attr] public string DeclarantId { get; set; }

    [Attr] public string DeclarantName { get; set; }

    [Attr] public string DispatchCountryCode { get; set; }

    [Attr] public string GoodsLocationCode { get; set; }

    [Attr] public List<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();

    //[Attr]
    //public Dictionary<string, TdmRelationshipObject> Relationships { get; set; } =
    //    new() { { "notifications", TdmRelationshipObject.CreateDefault() } };

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
    public List<int> _MatchReferences
    {
        get
        {
            var list = new HashSet<int>();
            if (Items is not null)
            {
                foreach (var item in Items)
                {
                    if (item.Documents != null)
                    {
                        foreach (var itemDocument in item.Documents)
                        {
                            list.Add(MatchingReferenceNumber
                                .FromCds(itemDocument.DocumentReference, itemDocument.DocumentCode).Identifier);
                        }
                    }
                }
            }

            matchReferences = list.ToList();
            return matchReferences;
        }
        set => matchReferences = value;
    }

    public void AddRelationship(string type, TdmRelationshipObject relationship)
    {
        Relationships.Notifications.Links ??= relationship.Links;
        foreach (var dataItem in relationship.Data)
        {
            if (Relationships.Notifications.Data.All(x => x.Id != dataItem.Id))
            {
                Relationships.Notifications.Data.Add(dataItem);
            }
        }

        Relationships.Notifications.Matched = Items
            .Select(x => x.ItemNumber)
            .All(itemNumber => Relationships.Notifications.Data.Any(x => x.Matched && x.SourceItem == itemNumber));
    }

    public void Update(AuditEntry auditEntry)
    {
        this.AuditEntries.Add(auditEntry);
        _Ts = DateTime.UtcNow;
    }

    public bool MergeDecision(string path, AlvsClearanceRequest clearanceRequest)
    {
        var before = this.ToJsonString();
        foreach (var item in clearanceRequest.Items)
        {
            var existingItem = this.Items.FirstOrDefault(x => x.ItemNumber == item.ItemNumber);

            if (existingItem is not null)
            {
                existingItem.MergeChecks(item);
            }
        }

        var after = this.ToJsonString();

        var auditEntry = AuditEntry.CreateDecision(before, after,
            BuildNormalizedDecisionPath(path),
            clearanceRequest.Header.EntryVersionNumber.GetValueOrDefault(),
            clearanceRequest.ServiceHeader.ServiceCalled,
            clearanceRequest.Header.DeclarantName);
        if (auditEntry.Diff.Any())
        {
            Decisions ??= new List<AlvsClearanceRequest>();
            Decisions.Add(clearanceRequest);
            this.Update(auditEntry);
        }

        return auditEntry.Diff.Any();
    }

    private string BuildNormalizedDecisionPath(string fullPath)
    {
        return fullPath.Replace("RAW/DECISIONS/", "");
    }

    public string? StringId { get; set; }
    public string? LocalId { get; set; }
    public string? Id { get; set; }
    public string _Etag { get; set; }
}