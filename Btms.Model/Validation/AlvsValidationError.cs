using Btms.Model.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace Btms.Model.Validation;
public class AlvsValidationError : IDataEntity
{
    [BsonId] public string? Id { get; set; } = null!;

    public required string Type { get; set; }
    public string _Etag { get; set; } = null!;

    public DateTime Created { get; set; }

    public DateTime UpdatedEntity { get; set; }

    public required BtmsValidationResult ValidationResult { get; set; }

    public required object Data { get; set; }
}