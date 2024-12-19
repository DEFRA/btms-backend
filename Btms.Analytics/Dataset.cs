using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Btms.Analytics;

// A marker interface to identify things we want to be able to return from the analytics API
public interface IDataset;


public class MultiSeriesDataset : IDataset
{
    public List<Series> Series { get; set; } = [];
}

public class SingleSeriesDataset : IDataset
{
    public IDictionary<string, int> Values { get; set; } = new Dictionary<string, int>();
}

public class TabularDataset<TColumn> : IDataset where TColumn : IDimensionResult
{
    public required List<TabularDimensionRow<TColumn>> Rows { get; set; }
}

public class EntityDataset<T>(IEnumerable<T> items) : IDataset
{
    public IEnumerable<T> Items { get; set; } = items;
}

public class MultiSeriesDatetimeDataset : IDataset
{
    public List<DatetimeSeries> Series { get; set; } = [];
}

/// <summary>
/// Serialise the derived types of IDataset
/// </summary>
/// <typeparam name="TType"></typeparam>
public class DatasetResultTypeMappingConverter<TType> : JsonConverter<TType> where TType : IDataset
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
        else if (value is TabularDataset<ByNameDimensionResult>)
        {
            JsonSerializer.Serialize(writer, value as TabularDataset<ByNameDimensionResult>, options);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}