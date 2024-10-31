
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum CommodityRiskResultPhsiClassificationEnum
{

		[EnumMember(Value = "Mandatory")]
		Mandatory,
	
		[EnumMember(Value = "Reduced")]
		Reduced,
	
		[EnumMember(Value = "Controlled")]
		Controlled,
	
}


