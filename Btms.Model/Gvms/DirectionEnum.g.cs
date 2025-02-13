
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Gvms;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DirectionEnum
{

    UkInbound,

    UkOutbound,

    GbToNi,

    NiToGb,

}