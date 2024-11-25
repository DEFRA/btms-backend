using Cdms.Model.Auditing;
using Cdms.Model.Data;
using Cdms.Model.Relationships;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cdms.Model.Gvms;

/// <summary>
/// 
/// </summary>
[Resource]
public partial class Gmr : IMongoIdentifiable, IDataEntity
{
    [JsonIgnore] public string Type { get; set; } = "gmrs";

    public string _Etag { get; set; } = default!;
    [Attr] public DateTime? CreatedSource { get; set; }
    [Attr] public DateTime Created { get; set; }
    [Attr] public DateTime Updated { get; set; }

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

    [Attr]
    [JsonPropertyName("relationships")]
    public GmrRelationships Relationships { get; set; } = new GmrRelationships();
}