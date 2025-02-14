
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LaboratoryTestsTestReasonEnum
{

    Random,

    Suspicious,

    ReEnforced,

    IntensifiedControls,

    Required,

    LatentInfectionSampling,

}