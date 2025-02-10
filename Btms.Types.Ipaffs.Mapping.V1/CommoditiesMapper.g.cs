//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//</auto-generated>
//------------------------------------------------------------------------------
#nullable enable


namespace Btms.Types.Ipaffs.Mapping;

public static class CommoditiesMapper
{
    public static Btms.Model.Ipaffs.Commodities Map(Btms.Types.Ipaffs.Commodities from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.Commodities();
        to.GmsDeclarationAccepted = from?.GmsDeclarationAccepted;
        to.ConsignedCountryInChargeGroup = from?.ConsignedCountryInChargeGroup;
        to.TotalGrossWeight = from?.TotalGrossWeight;
        to.TotalNetWeight = from?.TotalNetWeight;
        to.TotalGrossVolume = from?.TotalGrossVolume;
        to.TotalGrossVolumeUnit = from?.TotalGrossVolumeUnit;
        to.NumberOfPackages = from?.NumberOfPackages;
        to.Temperature = from?.Temperature;
        to.NumberOfAnimals = from?.NumberOfAnimals;
        to.CommodityComplements = from?.CommodityComplements?.Select(x => CommodityComplementMapper.Map(x)).ToArray();
        to.ComplementParameterSets = from?.ComplementParameterSets?.Select(x => ComplementParameterSetMapper.Map(x)).ToArray();
        to.IncludeNonAblactedAnimals = from?.IncludeNonAblactedAnimals;
        to.CountryOfOrigin = from?.CountryOfOrigin;
        to.CountryOfOriginIsPodCountry = from?.CountryOfOriginIsPodCountry;
        to.IsLowRiskArticle72Country = from?.IsLowRiskArticle72Country;
        to.RegionOfOrigin = from?.RegionOfOrigin;
        to.ConsignedCountry = from?.ConsignedCountry;
        to.AnimalsCertifiedAs = from?.AnimalsCertifiedAs;
        to.CommodityIntendedFor = CommoditiesCommodityIntendedForEnumMapper.Map(from?.CommodityIntendedFor);
        return to;
    }
}