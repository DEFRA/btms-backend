
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum UnitEnum
{

    [EnumMember(Value = "percent")]
    Percent,

    [EnumMember(Value = "number")]
    Number,

}