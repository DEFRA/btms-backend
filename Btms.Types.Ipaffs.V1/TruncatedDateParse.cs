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
        Debug.Assert(typeToConvert == typeof(DateOnly?));
        var s = reader.GetString();
        if (!s.HasValue()) return null;
        
        if (s!.Length > 10)
        {
            // logger.LogWarning("Received data that's supposed to be a date but needs truncating {Text}", s);
            s = s.Substring(0, 10);
        }
        
        return DateOnly.Parse(s!, new CultureInfo("en-GB"));
    }
    
    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteString("test", value!.Value.ToLongDateString());    
        }
    }
}