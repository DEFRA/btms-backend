
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Gvms;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum DirectionEnum
{

    [EnumMember(Value = "UK_INBOUND")]
    UkInbound,

    [EnumMember(Value = "UK_OUTBOUND")]
    UkOutbound,

    [EnumMember(Value = "GB_TO_NI")]
    GbToNi,

    [EnumMember(Value = "NI_TO_GB")]
    NiToGb,

}