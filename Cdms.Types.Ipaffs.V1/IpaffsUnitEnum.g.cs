
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum IpaffsUnitEnum
{

		[EnumMember(Value = "percent")]
		Percent,
	
		[EnumMember(Value = "number")]
		Number,
	
}


