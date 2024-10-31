
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Cdms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum DecisionNotAcceptableActionReDispatchReasonEnum
{

		[EnumMember(Value = "ContaminatedProducts")]
		ContaminatedProducts,
	
		[EnumMember(Value = "InterceptedPart")]
		InterceptedPart,
	
		[EnumMember(Value = "PackagingMaterial")]
		PackagingMaterial,
	
		[EnumMember(Value = "MeansOfTransport")]
		MeansOfTransport,
	
		[EnumMember(Value = "Other")]
		Other,
	
}


