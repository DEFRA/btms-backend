
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum DecisionDefinitiveImportPurposeEnum
{

    [EnumMember(Value = "slaughter")]
    Slaughter,

    [EnumMember(Value = "approvedbodies")]
    Approvedbodies,

    [EnumMember(Value = "quarantine")]
    Quarantine,

}