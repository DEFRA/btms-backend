using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Cdms.Model.Auditing;
using Cdms.Model.Data;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Cdms.Model.Gvms;

/// <summary>
/// 
/// </summary>
[Resource]
public partial class Gmr : IMongoIdentifiable, IDataEntity
{
    [JsonIgnore] public string Type { get; set; } = "gmrs";

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