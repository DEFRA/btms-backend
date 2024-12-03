
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ChedppNotAcceptableReasonCommodityOrPackageEnum
{

		[EnumMember(Value = "c")]
		C,
	
		[EnumMember(Value = "p")]
		P,
	
		[EnumMember(Value = "cp")]
		Cp,
	
}


