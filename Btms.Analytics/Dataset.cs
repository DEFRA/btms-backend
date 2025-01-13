using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Btms.Analytics;

// A marker interface to identify things we want to be able to return from the analytics API
public interface IDataset;

public class SummarisedDataset<TSummary, TResult> : IDataset
    where TResult : IDimensionResult
    where TSummary : IDimensionResult
{
    public required TSummary Summary { get; set; }
    public required List<TResult> Result { get; set; }
    
}

public class MultiSeriesDataset<TDimensionResult> : IDataset
    where TDimensionResult : IDimensionResult
{
    public List<Series<TDimensionResult>> Series { get; set; } = [];
}

public class SingleSeriesDataset : IDataset, IDimensionResult
{
    public IDictionary<string, int> Values { get; set; } = new Dictionary<string, int>();
}

public class TabularDataset<TColumn> : IDataset where TColumn : IDimensionResult
{
    public required List<TabularDimensionRow<TColumn>> Rows { get; set; }
}

public class EntityDataset<T>(IEnumerable<T> items) : IDataset
    where T : IDimensionResult
{
    public IEnumerable<T> Items { get; set; } = items;
}

public class MultiSeriesDatetimeDataset : IDataset
{
    public List<DatetimeSeries> Series { get; set; } = [];
}

public class ScenarioItem : IDimensionResult
{
    [JsonInclude]
    public required string Scenario;
    
    [JsonInclude]
    public required string[] Keys;
}

/// <summary>
/// Serialise the derived types of IDataset
/// </summary>
/// <typeparam name="TType"></typeparam>
public class DatasetResultTypeMappingConverter<TType> : JsonConverter<TType> where TType : IDataset
{
    [return: MaybeNull]
    public override TType Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var converters = options
            .Converters
            .Where(c => c is not DatasetResultTypeMappingConverter<TType>);

        var newOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = options.PropertyNamingPolicy
        };
        
        foreach (var jsonConverter in converters)
        {
            newOptions.Converters.Add(jsonConverter);
        }
        // newOptions.Converters.Append<IEnumerable<JsonConverter>>(converters);
        
        TType result = JsonSerializer.Deserialize<TType>(ref reader, newOptions)!;

        // TType result = JsonSerializer.Deserialize<TType>(ref reader, options)!;
        
        // return new ByNumericDimensionResult() { Dimension = 1, Value = 1};
        return result;   
    }

    public override void Write(Utf8JsonWriter writer, TType value, JsonSerializerOptions options)
    {
        if (value is MultiSeriesDatetimeDataset)
        {
            JsonSerializer.Serialize(writer, value as MultiSeriesDatetimeDataset, options);
        }
        else if (value is MultiSeriesDataset<ByNumericDimensionResult>)
        {
            JsonSerializer.Serialize(writer, value as MultiSeriesDataset<ByNumericDimensionResult>, options);
        }
        else if (value is SingleSeriesDataset)
        {
            JsonSerializer.Serialize(writer, value as SingleSeriesDataset, options);
        }
        else if (value is TabularDataset<ByNameDimensionResult>)
        {
            JsonSerializer.Serialize(writer, value as TabularDataset<ByNameDimensionResult>, options);
        }
        else if (value is SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>)
        {
            JsonSerializer.Serialize(writer, value as SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>, options);
        }
        // else if (value is EntityDataset<(string scenario, string ched, string[] mrns)>)
        // {
        //     JsonSerializer.Serialize(writer, value as EntityDataset<(string scenario, string ched, string[] mrns)>, options);
        // }
        
        else
        {
            throw new NotImplementedException();
        }
    }
}