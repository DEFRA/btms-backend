
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ConsignmentCheckIdentityCheckTypeEnum
{

		[EnumMember(Value = "Seal Check")]
		SealCheck,
	
		[EnumMember(Value = "Full Identity Check")]
		FullIdentityCheck,
	
		[EnumMember(Value = "Not Done")]
		NotDone,
	
}


