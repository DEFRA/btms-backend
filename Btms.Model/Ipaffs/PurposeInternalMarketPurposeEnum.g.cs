
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PurposeInternalMarketPurposeEnum
{

    AnimalFeedingStuff,

    HumanConsumption,

    PharmaceuticalUse,

    TechnicalUse,

    Other,

    CommercialSale,

    CommercialSaleOrChangeOfOwnership,

    Rescue,

    Breeding,

    Research,

    RacingOrCompetition,

    ApprovedPremisesOrBody,

    CompanionAnimalNotForResaleOrRehoming,

    Production,

    Slaughter,

    Fattening,

    GameRestocking,

    RegisteredHorses,

}