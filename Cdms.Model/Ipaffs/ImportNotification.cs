using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Cdms.Model.Auditing;
using Cdms.Model.Data;
using Cdms.Model.Relationships;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Cdms.Model.Ipaffs;

[Resource(PublicName = "import-notifications")]
public partial class ImportNotification : IMongoIdentifiable, IDataEntity
{
    private string? matchReference;

    //// This field is used by the jsonapi-consumer to control the correct casing in the type field
    [JsonIgnore] public string Type { get; set; } = "import-notification";

    //[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    [JsonIgnore]
    [Attr]
    public virtual string Id
    {
        get => ReferenceNumber;
        set => ReferenceNumber = value;
    }

    public string _Etag { get; set; }

    [Attr] 
    public DateTime Created { get; set; }

    [Attr] 
    public DateTime Updated { get; set; }

    // TODO : this is currently being written on the wire by the json api client
    /// <inheritdoc />
    
    [BsonIgnore]
    [NotMapped]
    [Attr]
    public string StringId
    {
        get => Id;
        set => Id = value;
    }

    /// <inheritdoc />
    [BsonIgnore]
    [NotMapped]
    // [Attr]
    public string LocalId { get; set; }

    [Attr] public List<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();

    [Attr]
    [JsonPropertyName("relationships")]
    public NotificationTdmRelationships Relationships { get; set; } = new NotificationTdmRelationships();

    [Attr] public Commodities CommoditiesSummary { get; set; }

    [Attr] public CommodityComplement[] Commodities { get; set; }

    // Filter fields...
    // These fields are added to the model solely for use by the filtering
    // Mechanism in JSON API as a short term solution until we implement the more complex nested filtering
    // https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/issues/76
    // They are removed from the document that is sent to the client by the JsonApiResourceDefinition OnApplySparseFieldSet
    // mechanism


    [Attr]
    [BsonElement("_pointOfEntry")]
    public string _PointOfEntry
    {
        get => PartOne?.PointOfEntry;
        set
        {
            if (PartOne != null)
            {
                PartOne.PointOfEntry = value;
            }
        }
    }

    [Attr]
    [BsonElement("_pointOfEntryControlPoint")]
    public string _PointOfEntryControlPoint
    {
        get => PartOne?.PointOfEntryControlPoint;
        set
        {
            if (PartOne != null)
            {
                PartOne.PointOfEntryControlPoint = value;
            }
        }
    }

    [BsonElement("_matchReferences")]
    public string _MatchReference
    {
        get
        {
            if (matchReference is null)
            {
                matchReference = MatchIdentifier.FromNotification(ReferenceNumber)
                    .Identifier;
            }

            return matchReference!;
        }
        set => matchReference = value;
    }

    public void AddRelationship(string type, TdmRelationshipObject relationship)
    {
        Relationships.Movements.Links ??= relationship.Links;
        foreach (var dataItem in relationship.Data)
        {
            if (Relationships.Movements.Data.All(x => x.Id != dataItem.Id))
            {
                Relationships.Movements.Data.Add(dataItem);
            }
        }

        Relationships.Movements.Matched = Relationships.Movements.Data.Any(x => x.Matched.GetValueOrDefault());
    }

    public void Changed(AuditEntry auditEntry)
    {
        this.AuditEntries.Add(auditEntry);
    }

    public void Create(string auditId)
    {
        var auditEntry = AuditEntry.CreateCreatedEntry(
            this,
            auditId,
            this.Version.GetValueOrDefault(),
            this.UpdatedSource);
        this.Changed(auditEntry);
    }

    public void Skipped(string auditId, int version)
    {
        var auditEntry = AuditEntry.CreateSkippedVersion(
            auditId,
            version,
            this.UpdatedSource);
        this.Changed(auditEntry);
    }

    public void Update(string auditId, ImportNotification previous)
    {
        var auditEntry = AuditEntry.CreateUpdated(previous,
            this,
            auditId,
            this.Version.GetValueOrDefault(),
            this.UpdatedSource);
        this.Changed(auditEntry);
    }
}