
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ControlConsignmentLeaveEnum
{

		[EnumMember(Value = "YES")]
		Yes,
	
		[EnumMember(Value = "NO")]
		No,
	
		[EnumMember(Value = "It has been destroyed")]
		ItHasBeenDestroyed,
	
}


