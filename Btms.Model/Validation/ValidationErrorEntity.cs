using Btms.Model.Data;
using Btms.Validation;
using MongoDB.Bson.Serialization.Attributes;

namespace Btms.Model.Validation;


public class ValidationErrorEntity : IDataEntity
{
    [BsonId]
    public string? Id { get; set; } = null!;

    public string _Etag { get; set; } = null!;

    public DateTime Created { get; set; }

    public DateTime UpdatedEntity { get; set; }

    public BtmsValidationResult? ValidationResult { get; set; }

    public object? Data { get; set; }
}