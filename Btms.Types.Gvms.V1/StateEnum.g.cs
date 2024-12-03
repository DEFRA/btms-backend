
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Gvms;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum StateEnum
{

		[EnumMember(Value = "NOT_FINALISABLE")]
		NotFinalisable,
	
		[EnumMember(Value = "OPEN")]
		Open,
	
		[EnumMember(Value = "FINALISED")]
		Finalised,
	
		[EnumMember(Value = "CHECKED_IN")]
		CheckedIn,
	
		[EnumMember(Value = "EMBARKED")]
		Embarked,
	
		[EnumMember(Value = "COMPLETED")]
		Completed,
	
}


