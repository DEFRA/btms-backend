using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdms.Types.Alvs.V1;

public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTime));

        ulong number = 0;

        if (reader.TokenType == JsonTokenType.Number)
        {
            reader.TryGetUInt64(out number);
        }
        else
        {
            var s = reader.GetString();
            if (!ulong.TryParse(s, out number))
            {
                return DateTime.Parse(s!, new CultureInfo("en-GB"));
            }
        }

        var s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 1723127967 - DEV
        // 1712851200000 - SND
        if (number > 10000000000)
        {
            return s_epoch.AddMilliseconds(number);
        }
        else if (number > 0)
        {
            return s_epoch.AddSeconds(number);
        }

        return s_epoch;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}