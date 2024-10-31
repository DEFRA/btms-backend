
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum EconomicOperatorStatusEnum
{

		[EnumMember(Value = "approved")]
		Approved,
	
		[EnumMember(Value = "nonapproved")]
		Nonapproved,
	
		[EnumMember(Value = "suspended")]
		Suspended,
	
}


