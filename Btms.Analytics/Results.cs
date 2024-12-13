using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

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

public class SingeSeriesDataset : IDataset
{
    public IDictionary<string, int> Values { get; set; } = new Dictionary<string, int>();
}

public class TypeMappingConverter<TType, TImplementation> : JsonConverter<TType>
    where TImplementation : TType
{
    [return: MaybeNull]
    public override TType Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<TImplementation>(ref reader, options);

    public override void Write(
        Utf8JsonWriter writer, TType value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, (TImplementation)value!, options);
}

public interface IDataset;

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