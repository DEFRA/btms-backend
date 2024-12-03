
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ImportNotificationTypeEnum
{

		[EnumMember(Value = "CVEDA")]
		Cveda,
	
		[EnumMember(Value = "CVEDP")]
		Cvedp,
	
		[EnumMember(Value = "CHEDPP")]
		Chedpp,
	
		[EnumMember(Value = "CED")]
		Ced,
	
		[EnumMember(Value = "IMP")]
		Imp,
	
}


