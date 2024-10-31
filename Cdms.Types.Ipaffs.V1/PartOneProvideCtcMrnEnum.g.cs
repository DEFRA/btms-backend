
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum PartOneProvideCtcMrnEnum
{

		[EnumMember(Value = "YES")]
		Yes,
	
		[EnumMember(Value = "YES_ADD_LATER")]
		YesAddLater,
	
		[EnumMember(Value = "NO")]
		No,
	
}


