using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Model.Auditing;

namespace Btms.Analytics;

public class ByDateTimeResult
{
    public DateTime Period { get; set; }
    public int Value { get; set; }
}

public interface IDimensionResult;

public class ByNumericDimensionResult : IDimensionResult
{
    public int Dimension { get; set; }
    public int Value { get; set; }
}

public class ByNameDimensionResult : IDimensionResult
{
    public required string Name { get; set; }
    public int Value { get; set; }
}

public class AuditHistory(AuditEntry auditEntry, string resourceType, string resourceId)
{
    public AuditEntry AuditEntry { get; set; } = auditEntry;
    public string ResourceType { get; set; } = resourceType;
    public string ResourceId { get; set; } = resourceId;
}

public class DatetimeSeries(string name)
{
    public string Name { get; set; } = name;
    public List<ByDateTimeResult> Periods { get; set; } = [];
}

public class Series
{
    public required string Name { get; set; }
    public required string Dimension { get; set; }
    public required List<IDimensionResult> Results { get; set; }
}

/// <summary>
/// Serialise the derived types of IDimensionResult
/// </summary>
/// <typeparam name="TType"></typeparam>
public class DimensionResultTypeMappingConverter<TType> : JsonConverter<TType> where TType : IDimensionResult
{
    [return: MaybeNull]
    public override TType Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, TType value, JsonSerializerOptions options)
    {
        if (value is ByNumericDimensionResult)
        {
            JsonSerializer.Serialize(writer, value as ByNumericDimensionResult, options);
        }
        else if (value is ByNameDimensionResult)
        {
            JsonSerializer.Serialize(writer, value as ByNameDimensionResult, options);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}