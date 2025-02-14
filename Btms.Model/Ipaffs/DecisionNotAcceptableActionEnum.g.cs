
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DecisionNotAcceptableActionEnum
{

    Slaughter,

    Reexport,

    Euthanasia,

    Redispatching,

    Destruction,

    Transformation,

    Other,

    EntryRefusal,

    QuarantineImposed,

    SpecialTreatment,

    IndustrialProcessing,

    ReDispatch,

    UseForOtherPurposes,

}