/*------------------------------------------------------------------------------
<auto-generated>
	This code was generated from the Enum template.
	Manual changes to this file may cause unexpected behavior in your application.
	Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum DecisionIfChanneledOptionEnum
{
    [EnumMember(Value = "article8")]
    Article8,
    [EnumMember(Value = "article15")]
    Article15,
}
