using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Btms.Types.Ipaffs;

public class TruncatedDateParse : JsonConverter<DateOnly?>
{
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert != typeof(DateOnly?)) return null;

        var s = reader.GetString();
        if (!s.HasValue()) return null;

        if (s!.Length > 10)
        {
            s = s.Substring(0, 10);
        }

        return DateOnly.Parse(s!, new CultureInfo("en-GB"));
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value!.Value.ToString("yyyy-MM-dd"));
        }
    }
}