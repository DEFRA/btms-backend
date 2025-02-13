//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
namespace Btms.Types.Ipaffs.Mapping;

public static class AnimalCertificationEnumMapper
{
    public static Btms.Model.Ipaffs.AnimalCertificationEnum? Map(Btms.Types.Ipaffs.AnimalCertificationEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.AnimalCertificationEnum.AnimalFeedingStuff => Btms.Model.Ipaffs.AnimalCertificationEnum.AnimalFeedingStuff,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Approved => Btms.Model.Ipaffs.AnimalCertificationEnum.Approved,
            Btms.Types.Ipaffs.AnimalCertificationEnum.ArtificialReproduction => Btms.Model.Ipaffs.AnimalCertificationEnum.ArtificialReproduction,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Breeding => Btms.Model.Ipaffs.AnimalCertificationEnum.Breeding,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Circus => Btms.Model.Ipaffs.AnimalCertificationEnum.Circus,
            Btms.Types.Ipaffs.AnimalCertificationEnum.CommercialSale => Btms.Model.Ipaffs.AnimalCertificationEnum.CommercialSale,
            Btms.Types.Ipaffs.AnimalCertificationEnum.CommercialSaleOrChangeOfOwnership => Btms.Model.Ipaffs.AnimalCertificationEnum.CommercialSaleOrChangeOfOwnership,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Fattening => Btms.Model.Ipaffs.AnimalCertificationEnum.Fattening,
            Btms.Types.Ipaffs.AnimalCertificationEnum.GameRestocking => Btms.Model.Ipaffs.AnimalCertificationEnum.GameRestocking,
            Btms.Types.Ipaffs.AnimalCertificationEnum.HumanConsumption => Btms.Model.Ipaffs.AnimalCertificationEnum.HumanConsumption,
            Btms.Types.Ipaffs.AnimalCertificationEnum.InternalMarket => Btms.Model.Ipaffs.AnimalCertificationEnum.InternalMarket,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Other => Btms.Model.Ipaffs.AnimalCertificationEnum.Other,
            Btms.Types.Ipaffs.AnimalCertificationEnum.PersonallyOwnedPetsNotForRehoming => Btms.Model.Ipaffs.AnimalCertificationEnum.PersonallyOwnedPetsNotForRehoming,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Pets => Btms.Model.Ipaffs.AnimalCertificationEnum.Pets,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Production => Btms.Model.Ipaffs.AnimalCertificationEnum.Production,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Quarantine => Btms.Model.Ipaffs.AnimalCertificationEnum.Quarantine,
            Btms.Types.Ipaffs.AnimalCertificationEnum.RacingCompetition => Btms.Model.Ipaffs.AnimalCertificationEnum.RacingCompetition,
            Btms.Types.Ipaffs.AnimalCertificationEnum.RegisteredEquidae => Btms.Model.Ipaffs.AnimalCertificationEnum.RegisteredEquidae,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Registered => Btms.Model.Ipaffs.AnimalCertificationEnum.Registered,
            Btms.Types.Ipaffs.AnimalCertificationEnum.RejectedOrReturnedConsignment => Btms.Model.Ipaffs.AnimalCertificationEnum.RejectedOrReturnedConsignment,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Relaying => Btms.Model.Ipaffs.AnimalCertificationEnum.Relaying,
            Btms.Types.Ipaffs.AnimalCertificationEnum.RescueRehoming => Btms.Model.Ipaffs.AnimalCertificationEnum.RescueRehoming,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Research => Btms.Model.Ipaffs.AnimalCertificationEnum.Research,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Slaughter => Btms.Model.Ipaffs.AnimalCertificationEnum.Slaughter,
            Btms.Types.Ipaffs.AnimalCertificationEnum.TechnicalPharmaceuticalUse => Btms.Model.Ipaffs.AnimalCertificationEnum.TechnicalPharmaceuticalUse,
            Btms.Types.Ipaffs.AnimalCertificationEnum.Transit => Btms.Model.Ipaffs.AnimalCertificationEnum.Transit,
            Btms.Types.Ipaffs.AnimalCertificationEnum.ZooCollection => Btms.Model.Ipaffs.AnimalCertificationEnum.ZooCollection,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}