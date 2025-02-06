using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Btms.Model;

public class LocalDateTimeNoSecondsJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture));
    }
}