using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Btms.Model;

public class LocalDateTimeJsonConverter : JsonConverter<DateTime>
{
    private const string JsonFormat = "yyyy-MM-ddTHH:mm:ss";
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTimeFromJson = reader.GetString()!;
        return DateTime.SpecifyKind(DateTime.ParseExact(dateTimeFromJson, JsonFormat, CultureInfo.InvariantCulture), DateTimeKind.Unspecified);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        if (value.Kind != DateTimeKind.Unspecified)
        {
            throw new FormatException($"Local dates must be DateTimeKind.Unspecified, not {value.Kind}");
        }
        writer.WriteStringValue(value.ToString(JsonFormat, CultureInfo.InvariantCulture));
    }
}