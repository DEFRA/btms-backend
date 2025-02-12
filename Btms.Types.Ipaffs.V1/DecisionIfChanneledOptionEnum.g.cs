
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum DecisionIfChanneledOptionEnum
{

    [EnumMember(Value = "article8")]
    Article8,

    [EnumMember(Value = "article15")]
    Article15,

}