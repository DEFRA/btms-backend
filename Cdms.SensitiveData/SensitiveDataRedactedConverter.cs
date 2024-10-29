using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdms.SensitiveData;

public class SensitiveDataRedactedConverter(SensitiveDataOptions sensitiveDataOptions) : JsonConverter<string>
{
    /// <inheritdoc/>
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return sensitiveDataOptions.Getter(reader.GetString());
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, string objectToWrite, JsonSerializerOptions options)
        => throw new NotImplementedException("Only for reading");
}