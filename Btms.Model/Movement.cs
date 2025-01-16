using Btms.Model.Extensions;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;
using Btms.Model.Cds;
using Btms.Model.Auditing;
using Btms.Model.ChangeLog;
using Btms.Model.Data;
using Btms.Model.Relationships;
using Microsoft.Extensions.Logging;

namespace Btms.Model;

// Recreation of ClearanceRequest schema from
// https://eaflood.atlassian.net/wiki/spaces/TRADE/pages/5104664583/PHA+Port+Health+Authority+Integration+Data+Schema

[Resource]
public class Movement : IMongoIdentifiable, IDataEntity, IAuditable
{
    private List<string> matchReferences = [];

    [ChangeSetIgnore] //TODO : should we ignore this or not?
    [Attr]
    public required MovementStatus BtmsStatus { get; set; } = MovementStatus.Default();
        
    // This field is used by the jsonapi-consumer to control the correct casing in the type field
    [ChangeSetIgnore]
    public string Type { get; set; } = "movements";

    [Attr] public List<CdsClearanceRequest> ClearanceRequests { get; init; } = default!;

    [Attr] public List<CdsClearanceRequest> Decisions { get; set; } = default!;

    [Attr] public AlvsDecisionStatus AlvsDecisionStatus { get; set; } = new AlvsDecisionStatus();
    
    [Attr] public List<Items> Items { get; set; } = [];

    [Attr]
    public DateTime? UpdatedSource { get; set; }
    [Attr] public DateTime? CreatedSource { get; set; }

    [Attr] public string EntryReference { get; set; } = default!;

    [Attr] public int EntryVersionNumber { get; set; } = default!;

    [Attr] public string MasterUcr { get; set; } = default!;

    [Attr] public int? DeclarationPartNumber { get; set; }

    [Attr] public string DeclarationType { get; set; } = default!;

    [Attr] public DateTime? ArrivesAt { get; set; }

    [Attr] public string SubmitterTurn { get; set; } = default!;

    [Attr] public string DeclarantId { get; set; } = default!;

    [Attr] public string DeclarantName { get; set; } = default!;

    [Attr] public string DispatchCountryCode { get; set; } = default!;

    [Attr] public string GoodsLocationCode { get; set; } = default!;

    [Attr] 
    [ChangeSetIgnore]
    public List<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();

    [Attr]
    [JsonPropertyName("relationships")]
    public MovementTdmRelationships Relationships { get; set; } = new MovementTdmRelationships();


    [BsonElement("_matchReferences")]
    public List<string> _MatchReferences
    {
        get
        {
            if (!matchReferences.Any())
            {
                var list = new HashSet<string>();

                foreach (var identifier in Items.SelectMany(item => item.GetIdentifiers()))
                {
                    list.Add(identifier);
                }

                matchReferences = list.ToList();
            }

            return matchReferences;
           
        }
        set => matchReferences = value;
    }

    public void AddRelationship(TdmRelationshipObject relationship)
    {
        bool linked = false;
        Relationships.Notifications.Links ??= relationship.Links;

        var dataItems = relationship.Data.Where(dataItem =>
            Relationships.Notifications.Data.TrueForAll(x => x.Id != dataItem.Id))
            .ToList();

        if (dataItems.Any())
        {
            Relationships.Notifications.Data.AddRange(dataItems);
            linked = true;
        }

        Relationships.Notifications.Matched = Items
            .Select(x => x.ItemNumber)
            .All(itemNumber =>
                Relationships.Notifications.Data.Exists(x => x.Matched.GetValueOrDefault() && x.SourceItem == itemNumber));

        //TODO : This would be the right time to call AddLinkStatus I think
        // but relies on linking being moved into Business
        // this.AddLinkStatus();
        
        if (linked)
        {
            AuditEntries.Add(AuditEntry.CreateLinked(String.Empty, this.AuditEntries.FirstOrDefault()?.Version ?? 1));
        }
    }

    public void RemoveRelationship(RelationshipDataItem relationship)
    {
        if (Relationships.Notifications.Data.Contains(relationship))
        {
            Relationships.Notifications.Data.Remove(relationship);
        }

        Relationships.Notifications.Matched = Relationships.Notifications.Data.TrueForAll(x => x.Matched.GetValueOrDefault());
    }
    
    [BsonIgnore]
    [NotMapped]
    [ChangeSetIgnore]
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

    [ChangeSetIgnore]
    public string _Etag { get; set; } = null!;

    [Attr]
    [ChangeSetIgnore]
    public DateTime Created { get; set; }

    [Attr]
    [ChangeSetIgnore] 
    public DateTime Updated { get; set; }
    public AuditEntry GetLatestAuditEntry()
    {
        return this.AuditEntries.OrderByDescending(x => x.CreatedLocal).First();
    }
}