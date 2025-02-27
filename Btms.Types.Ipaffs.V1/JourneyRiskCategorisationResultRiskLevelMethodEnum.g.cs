
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum JourneyRiskCategorisationResultRiskLevelMethodEnum
{

    [EnumMember(Value = "System")]
    System,

    [EnumMember(Value = "User")]
    User,

}