
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TemperatureEnum
{

		[EnumMember(Value = "Ambient")]
		Ambient,
	
		[EnumMember(Value = "Chilled")]
		Chilled,
	
		[EnumMember(Value = "Frozen")]
		Frozen,
	
}


