using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Btms.Types.Ipaffs;

public class KeyDataPairsToDictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object?>>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(IDictionary<string, object>)
               || typeToConvert == typeof(IDictionary<string, object?>);
    }

    public override Dictionary<string, object?> Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
        }

        var dictionary = new Dictionary<string, object?>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                reader.Skip();
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return dictionary;
            }

            if (reader.TokenType == JsonTokenType.StartArray || reader.TokenType == JsonTokenType.StartObject)
            {
                continue;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("JsonTokenType was not PropertyName");
            }

            var propertyName = reader.GetString();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new JsonException("Failed to get property name");
            }

            if (propertyName == "key")
            {
                ReadEntry(ref reader, options, dictionary);
            }
        }

        return dictionary;
    }

    private void ReadEntry(ref Utf8JsonReader reader, JsonSerializerOptions options, Dictionary<string, object?> dictionary)
    {
        //get key value
        reader.Read();
        var key = ExtractValue(ref reader, options);

        reader.Read();
        object? value = null;
        if (reader.TokenType != JsonTokenType.EndObject)
        {
            reader.Read();
            value = ExtractValue(ref reader, options);
        }


        dictionary.Add(key?.ToString()!, value);
    }

    public override void Write(
        Utf8JsonWriter writer, Dictionary<string, object?> value, JsonSerializerOptions options)
    {
        var list = value.Select(x => new KeyDataPair { Key = x.Key, Data = x.Value?.ToString() });
        JsonSerializer.Serialize(writer, list, options);
    }

    private static object ReadString(ref Utf8JsonReader reader)
    {
        var s = reader.GetString();
        int i;
        if (int.TryParse(s, out i))
        {
            return i;
        }

        return s!;
    }

    private object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                if (reader.TryGetDateTime(out var date))
                {
                    return date;
                }

                return ReadString(ref reader);
            case JsonTokenType.False:
                return false;
            case JsonTokenType.True:
                return true;
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var result))
                {
                    return result;
                }

                return reader.GetDecimal();
            case JsonTokenType.StartObject:
                return Read(ref reader, null!, options);
            case JsonTokenType.StartArray:
                var list = new List<object?>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    list.Add(ExtractValue(ref reader, options));
                }

                return list;
            default:
                throw new JsonException($"'{reader.TokenType}' is not supported");
        }
    }
}