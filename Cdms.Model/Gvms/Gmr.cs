using JsonApiDotNetCore.Resources.Annotations;
using System.Text.Json.Serialization;
using System.Dynamic;
using Cdms.Model.Data;
using Cdms.Model.Auditing;
using JsonApiDotNetCore.MongoDb.Resources;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;


namespace Cdms.Model.VehicleMovement;

/// <summary>
/// 
/// </summary>
[Resource]
public partial class Gmr : IMongoIdentifiable, IDataEntity
{
    [JsonIgnore] public string Type { get; set; } = "gmrs";

    [JsonIgnore]
    public virtual string? Id
    {
        get => GmrId;
        set => GmrId = value;
    }

    public string _Etag { get; set; }

    // TODO : this is currently being written on the wire by the json api client
    /// <inheritdoc />
    [BsonIgnore]
    [JsonIgnore]
    // [NotMapped]
    [Attr]
    public string? StringId
    {
        get => Id;
        set => Id = value;
    }

    /// <inheritdoc />
    [BsonIgnore]
    [JsonIgnore]
    [NotMapped]
    // [Attr]
    public string? LocalId { get; set; }

    [Attr] public List<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
}