
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum PartOneStoreTransporterContactEnum
{

    [EnumMember(Value = "YES")]
    Yes,

    [EnumMember(Value = "NO")]
    No,

}