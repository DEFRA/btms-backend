
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InspectionCheckStatusEnum
{

    ToDo,

    Compliant,

    AutoCleared,

    NonCompliant,

    NotInspected,

    ToBeInspected,

    Hold,

}