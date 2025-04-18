
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ConsignmentCheckIdentityCheckNotDoneReasonEnum
{

    [EnumMember(Value = "Reduced checks regime")]
    ReducedChecksRegime,

    [EnumMember(Value = "Not required")]
    NotRequired,

}