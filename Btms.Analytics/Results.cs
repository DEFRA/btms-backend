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

public class ByNumericDimensionResult
{
    public int Dimension { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Serialise the derived types of IDataset
/// </summary>
/// <typeparam name="TType"></typeparam>
public class ResultTypeMappingConverter<TType> : JsonConverter<TType> where TType : IDataset
{
    [return: MaybeNull]
    public override TType Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, TType value, JsonSerializerOptions options)
    {
        if (value is MultiSeriesDatetimeDataset)
        {
            JsonSerializer.Serialize(writer, value as MultiSeriesDatetimeDataset, options);
        }
        else if (value is MultiSeriesDataset)
        {
            JsonSerializer.Serialize(writer, value as MultiSeriesDataset, options);
        }
        else if (value is SingleSeriesDataset)
        {
            JsonSerializer.Serialize(writer, value as SingleSeriesDataset, options);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}

// A marker interface to identify things we want to be able to return from the analytics API
public interface IDataset;

public class SingleSeriesDataset : IDataset
{
    public IDictionary<string, int> Values { get; set; } = new Dictionary<string, int>();
}

public class AuditHistory(AuditEntry auditEntry, string resourceType, string resourceId)
{
    public AuditEntry AuditEntry { get; set; } = auditEntry;
    public string ResourceType { get; set; } = resourceType;
    public string ResourceId { get; set; } = resourceId;
}

public class EntityDataset<T>(IEnumerable<T> items) : IDataset
{
    public IEnumerable<T> Items { get; set; } = items;
}

public class MultiSeriesDatetimeDataset : IDataset
{
    public List<DatetimeSeries> Series { get; set; } = [];
}

public class DatetimeSeries(string name)
{
    public string Name { get; set; } = name;
    public List<ByDateTimeResult> Periods { get; set; } = [];
}

public class MultiSeriesDataset : IDataset
{
    public List<Series> Series { get; set; } = [];
}

public class Series(string name, string dimension)
{
    public string Name { get; set; } = name;
    public string Dimension { get; set; } = dimension;
    public List<ByNumericDimensionResult> Results { get; set; } = [];
}