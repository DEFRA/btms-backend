using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Model.Auditing;
using Btms.Model.Ipaffs;

namespace Btms.Analytics;

public class ByDateTimeResult
{
    public DateTime Period { get; set; }
    public int Value { get; set; }
}

public class ExceptionResult : IDimensionResult
{
    public required string Id { get; set; }
    public required string Resource { get; set; }
    public required DateTime UpdatedSource { get; set; }
    public required DateTime Updated { get; set; }
    
    public required int ItemCount { get; set; }
    public required ImportNotificationTypeEnum[] ChedTypes { get; set; }
    public required int MaxEntryVersion { get; set; }
    public required int MaxDecisionNumber { get; set; }
    public required int LinkedCheds { get; set; }
    public required string Reason { get; set; }
}

public interface IDimensionResult;

public class TabularDimensionRow<TColumn> where TColumn : IDimensionResult
{
    public required string Key { get; set; }
    public required List<TColumn> Columns { get; set; }
}

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

public class StringBucketDimensionResult : IDimensionResult
{
    public required Dictionary<string,string> Fields { get; set; }
    public int Value { get; set; }
}

public class AuditHistory(AuditEntry auditEntry, string resourceType, string resourceApiPrefix, string resourceId)
    : IDimensionResult 
{
    public AuditEntry AuditEntry { get; set; } = auditEntry;
    public string ResourceType { get; set; } = resourceType;
    public string ResourceApiPrefix { get; set; } = resourceApiPrefix;
    public string ResourceId { get; set; } = resourceId;
}

public class DatetimeSeries(string name)
{
    public string Name { get; set; } = name;
    public List<ByDateTimeResult> Periods { get; set; } = [];
}

public class Series<TDimensionResult>
{
    public required string Name { get; set; }
    public required string Dimension { get; set; }
    public required List<TDimensionResult> Results { get; set; }
}

/// <summary>
/// Serialise the derived types of IDimensionResult
/// </summary>
/// <typeparam name="TType"></typeparam>
public class DimensionResultTypeMappingConverter<TType> : JsonConverter<TType> where TType : IDimensionResult
{
    [return: MaybeNull]
    public override TType Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = options.PropertyNamingPolicy
        };
        // ByNumericDimensionResult byNumeric = JsonSerializer.Deserialize<ByNumericDimensionResult>(ref reader, newOptions)!;

        TType result = JsonSerializer.Deserialize<TType>(ref reader, newOptions)!;
        // return new ByNumericDimensionResult() { Dimension = 1, Value = 1};
        return result;
    }

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