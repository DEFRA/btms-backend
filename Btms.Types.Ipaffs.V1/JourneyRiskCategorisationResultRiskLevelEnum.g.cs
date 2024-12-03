
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum JourneyRiskCategorisationResultRiskLevelEnum
{

		[EnumMember(Value = "High")]
		High,
	
		[EnumMember(Value = "Medium")]
		Medium,
	
		[EnumMember(Value = "Low")]
		Low,
	
}


