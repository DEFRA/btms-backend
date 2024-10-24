
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum IpaffsDecisionIfChanneledOptionEnum
{

		[EnumMember(Value = "article8")]
		Article8,
	
		[EnumMember(Value = "article15")]
		Article15,
	
}


