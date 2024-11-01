
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Gvms;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum HaulierTypeEnum
{

		[EnumMember(Value = "STANDARD")]
		Standard,
	
		[EnumMember(Value = "FPO_ASN")]
		FpoAsn,
	
		[EnumMember(Value = "FPO_OTHER")]
		FpoOther,
	
		[EnumMember(Value = "NATO_MOD")]
		NatoMod,
	
		[EnumMember(Value = "RMG")]
		Rmg,
	
		[EnumMember(Value = "ETOE")]
		Etoe,
	
}


