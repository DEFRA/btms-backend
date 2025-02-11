
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum InspectionRequiredEnum
{

    [EnumMember(Value = "Required")]
    Required,

    [EnumMember(Value = "Inconclusive")]
    Inconclusive,

    [EnumMember(Value = "Not required")]
    NotRequired,

}