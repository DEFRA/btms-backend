using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Btms.Model.Auditing;
using Btms.Model.Data;
using Btms.Model.Relationships;
using Btms.Model.ChangeLog;

namespace Btms.Model.Ipaffs;

[Resource(PublicName = "import-notifications")]
public partial class ImportNotification : IMongoIdentifiable, IDataEntity, IAuditable, IResource
{
    private string? matchReference;

    //// This field is used by the jsonapi-consumer to control the correct casing in the type field
    [JsonIgnore]
    [ChangeSetIgnore]
    public string Type { get; set; } = "import-notification";

    /// <summary>
    /// Has a broader understanding of the status of the CHED and its relationships with other resources etc
    /// I'd like to name this Status but that already exists on the notification so need to review.
    /// </summary>
    [ChangeSetIgnore]
    [Attr]
    public ImportNotificationStatus BtmsStatus { get; set; } = ImportNotificationStatus.Default();

    //[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    [JsonIgnore]
    [Attr]
    public virtual string? Id
    {
        get => ReferenceNumber!;
        set => ReferenceNumber = value;
    }

    [ChangeSetIgnore]
    // ReSharper disable once InconsistentNaming - want to use Mongo DB convention to indicate none core schema properties
    public string _Etag { get; set; } = default!;

    [Attr]
    public DateTime? CreatedSource { get; set; }

    [Attr]
    [ChangeSetIgnore]
    public DateTime Created { get; set; }

    [Attr]
    [ChangeSetIgnore]
    public DateTime UpdatedEntity { get; set; }

    [Attr]
    [ChangeSetIgnore]
    public DateTime Updated { get; set; }

    [BsonIgnore]
    [NotMapped]
    [Attr]
    public string? StringId
    {
        get => Id;
        set => Id = value!;
    }

    /// <inheritdoc />
    [BsonIgnore]
    [NotMapped]
    // [Attr]
    public string? LocalId { get; set; } = default!;

    [Attr] public List<AuditEntry> AuditEntries { get; set; } = new();

    [Attr]
    [JsonPropertyName("relationships")]
    public NotificationTdmRelationships Relationships { get; set; } = new();

    [Attr] public Commodities CommoditiesSummary { get; set; } = default!;

    [Attr] public CommodityComplement[] Commodities { get; set; } = default!;

    // Filter fields...
    // These fields are added to the model solely for use by the filtering
    // Mechanism in JSON API as a short term solution until we implement the more complex nested filtering
    // https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/issues/76
    // They are removed from the document that is sent to the client by the JsonApiResourceDefinition OnApplySparseFieldSet
    // mechanism


    [Attr]
    [BsonElement("_pointOfEntry")]
    [ChangeSetIgnore]
    // ReSharper disable once InconsistentNaming - want to use Mongo DB convention to indicate none core schema properties
    public string _PointOfEntry
    {
        get => PartOne?.PointOfEntry!;
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
    [ChangeSetIgnore]
    // ReSharper disable once InconsistentNaming - want to use Mongo DB convention to indicate none core schema properties
    public string _PointOfEntryControlPoint
    {
        get => PartOne?.PointOfEntryControlPoint!;
        set
        {
            if (PartOne != null)
            {
                PartOne.PointOfEntryControlPoint = value;
            }
        }
    }

    [BsonElement("_matchReferences")]
    [ChangeSetIgnore]
    // ReSharper disable once InconsistentNaming - want to use Mongo DB convention to indicate none core schema properties
    public string _MatchReference
    {
        get
        {
            if (matchReference is null)
            {
                matchReference = MatchIdentifier.FromNotification(ReferenceNumber!)
                    .Identifier;
            }

            return matchReference!;
        }
        set => matchReference = value;
    }

    public void AddRelationship(TdmRelationshipObject relationship)
    {
        var linked = false;
        Relationships.Movements.Links ??= relationship.Links;

        var dataItems = relationship.Data.Where(dataItem =>
                Relationships.Movements.Data.TrueForAll(x => x.Id != dataItem.Id))
            .ToList();

        if (dataItems.Any())
        {
            Relationships.Movements.Data.AddRange(dataItems);
            linked = true;
        }

        if (linked)
        {
            AuditEntries.Add(AuditEntry.CreateLinked(string.Empty, Version.GetValueOrDefault()));
        }
    }

    public void RemoveRelationship(RelationshipDataItem relationship)
    {
        var unlinked = false;

        if (Relationships.Movements.Data.Contains(relationship))
        {
            Relationships.Movements.Data.Remove(relationship);
            unlinked = true;
        }

        if (unlinked)
        {
            AuditEntries.Add(AuditEntry.CreateUnlinked(string.Empty, Version.GetValueOrDefault(), UpdatedSource));
        }
    }

    public void RemoveAllRelationships()
    {
        Relationships.Movements = TdmRelationshipObject.CreateDefault();
        AuditEntries.Add(AuditEntry.CreateUnlinked(string.Empty, Version.GetValueOrDefault(), UpdatedSource));
    }

    public void Changed(AuditEntry auditEntry)
    {
        AuditEntries.Add(auditEntry);
        Updated = DateTime.UtcNow;
    }

    public void Create(string auditId)
    {
        var auditEntry = AuditEntry.CreateCreatedEntry(
            this,
            auditId,
            Version.GetValueOrDefault(),
            UpdatedSource,
            CreatedBySystem.Ipaffs);
        Changed(auditEntry);
    }

    public void Skipped(string auditId, int version)
    {
        var auditEntry = AuditEntry.CreateSkippedVersion(
            auditId,
            version,
            UpdatedSource,
            CreatedBySystem.Ipaffs);
        Changed(auditEntry);
    }

    public void Update(string auditId, ChangeSet changeSet)
    {
        var auditEntry = AuditEntry.CreateUpdated(changeSet,
            auditId,
            Version.GetValueOrDefault(),
            UpdatedSource,
            CreatedBySystem.Ipaffs);
        Changed(auditEntry);
    }

    public void Cancel(string auditId, ChangeSet changeSet)
    {
        var auditEntry = AuditEntry.CreateCancelled(changeSet,
            auditId,
            Version.GetValueOrDefault(),
            UpdatedSource,
            CreatedBySystem.Ipaffs);
        Changed(auditEntry);
    }

    public void Delete(string auditId, ChangeSet changeSet)
    {
        var auditEntry = AuditEntry.CreateDeleted(changeSet,
            auditId,
            Version.GetValueOrDefault(),
            UpdatedSource,
            CreatedBySystem.Ipaffs);
        Changed(auditEntry);
    }

    public AuditEntry GetLatestAuditEntry()
    {
        return AuditEntries.OrderByDescending(x => x.CreatedLocal).First();
    }

    public void CalculateStatus()
    {
        BtmsStatus.LinkStatus = (Relationships.Movements.Data.Count > 0) ? LinkStatus.Linked : LinkStatus.NotLinked;
        BtmsStatus.TypeAndLinkStatus = this switch
        {
            //Linked
            { BtmsStatus.LinkStatus: LinkStatus.Linked, ImportNotificationType: ImportNotificationTypeEnum.Cveda } => TypeAndLinkStatus.ChedALinked,
            { BtmsStatus.LinkStatus: LinkStatus.Linked, ImportNotificationType: ImportNotificationTypeEnum.Ced } => TypeAndLinkStatus.ChedDLinked,
            { BtmsStatus.LinkStatus: LinkStatus.Linked, ImportNotificationType: ImportNotificationTypeEnum.Cvedp } => TypeAndLinkStatus.ChedPLinked,
            { BtmsStatus.LinkStatus: LinkStatus.Linked, ImportNotificationType: ImportNotificationTypeEnum.Chedpp } => TypeAndLinkStatus.ChedPpLinked,

            //Not linked
            { BtmsStatus.LinkStatus: LinkStatus.NotLinked, ImportNotificationType: ImportNotificationTypeEnum.Cveda } => TypeAndLinkStatus.ChedANotLinked,
            { BtmsStatus.LinkStatus: LinkStatus.NotLinked, ImportNotificationType: ImportNotificationTypeEnum.Ced } => TypeAndLinkStatus.ChedDNotLinked,
            { BtmsStatus.LinkStatus: LinkStatus.NotLinked, ImportNotificationType: ImportNotificationTypeEnum.Cvedp } => TypeAndLinkStatus.ChedPNotLinked,
            { BtmsStatus.LinkStatus: LinkStatus.NotLinked, ImportNotificationType: ImportNotificationTypeEnum.Chedpp } => TypeAndLinkStatus.ChedPpNotLinked,
            _ => throw new NotImplementedException($"No mapping for LinkStatus={BtmsStatus.LinkStatus}, ImportNotificationType={ImportNotificationType}.")
        };
    }
}


public class ImportNotificationStatus
{
    public static ImportNotificationStatus Default()
    {
        return new ImportNotificationStatus()
        {
            LinkStatus = Ipaffs.LinkStatus.NotLinked
        };
    }

    [Attr]
    [System.ComponentModel.Description("")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public TypeAndLinkStatus? TypeAndLinkStatus { get; set; }

    [Attr]
    [System.ComponentModel.Description("")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public required LinkStatus LinkStatus { get; set; }
}