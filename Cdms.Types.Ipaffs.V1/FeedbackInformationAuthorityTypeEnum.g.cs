
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum FeedbackInformationAuthorityTypeEnum
{

		[EnumMember(Value = "exitbip")]
		Exitbip,
	
		[EnumMember(Value = "finalbip")]
		Finalbip,
	
		[EnumMember(Value = "localvetunit")]
		Localvetunit,
	
		[EnumMember(Value = "inspunit")]
		Inspunit,
	
}


