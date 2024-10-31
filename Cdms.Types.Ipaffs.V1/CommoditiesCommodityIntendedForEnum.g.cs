
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum CommoditiesCommodityIntendedForEnum
{

		[EnumMember(Value = "human")]
		Human,
	
		[EnumMember(Value = "feedingstuff")]
		Feedingstuff,
	
		[EnumMember(Value = "further")]
		Further,
	
		[EnumMember(Value = "other")]
		Other,
	
}


