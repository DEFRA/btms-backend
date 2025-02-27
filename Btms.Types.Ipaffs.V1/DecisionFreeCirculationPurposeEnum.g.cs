
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum DecisionFreeCirculationPurposeEnum
{

    [EnumMember(Value = "Animal Feeding Stuff")]
    AnimalFeedingStuff,

    [EnumMember(Value = "Human Consumption")]
    HumanConsumption,

    [EnumMember(Value = "Pharmaceutical Use")]
    PharmaceuticalUse,

    [EnumMember(Value = "Technical Use")]
    TechnicalUse,

    [EnumMember(Value = "Further Process")]
    FurtherProcess,

    [EnumMember(Value = "Other")]
    Other,

}