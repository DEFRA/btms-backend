
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum PartThreeControlStatusEnum
{

		[EnumMember(Value = "REQUIRED")]
		Required,
	
		[EnumMember(Value = "COMPLETED")]
		Completed,
	
}


