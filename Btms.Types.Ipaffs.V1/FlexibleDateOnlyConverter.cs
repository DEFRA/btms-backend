using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Btms.Types.Ipaffs;

/// <summary>
/// Handles DateOnly fields that might be specified in different formats in examples of the same message!
/// For example, occasionally we've seen IPAFFS AccompanyingDocument where DocumentIssueDate includes time info,
/// when most examples don't
/// One of our example documents 
// Samples/NoAuditLogForMovementUpdate/IPAFFS/CHEDP/2024/12/26/CHEDP_GB_2024_031218000001-81b1a60c-5800-41eb-84fc-f73cb99585ef.json
/// </summary>
public class FlexibleDateOnlyConverter : JsonConverter<DateOnly?>
{
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert != typeof(DateOnly?)) return null;

        return reader.GetDateTime().ToDate();
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value!.Value.ToString("o", CultureInfo.InvariantCulture));
        }
    }
}