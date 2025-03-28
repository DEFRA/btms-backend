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

public static class PurposeMapper
{
    public static Btms.Model.Ipaffs.Purpose Map(Btms.Types.Ipaffs.Purpose? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.Purpose();
        to.ConformsToEU = from?.ConformsToEU;
        to.InternalMarketPurpose = PurposeInternalMarketPurposeEnumMapper.Map(from?.InternalMarketPurpose);
        to.ThirdCountryTranshipment = from?.ThirdCountryTranshipment;
        to.ForNonConforming = PurposeForNonConformingEnumMapper.Map(from?.ForNonConforming);
        to.RegNumber = from?.RegNumber;
        to.ShipName = from?.ShipName;
        to.ShipPort = from?.ShipPort;
        to.ExitBip = from?.ExitBip;
        to.ThirdCountry = from?.ThirdCountry;
        to.TransitThirdCountries = from?.TransitThirdCountries;
        to.ForImportOrAdmission = PurposeForImportOrAdmissionEnumMapper.Map(from?.ForImportOrAdmission);
        to.ExitDate = from?.ExitDate;
        to.FinalBip = from?.FinalBip;
        to.PurposeGroup = PurposePurposeGroupEnumMapper.Map(from?.PurposeGroup);
        to.EstimatedArrivesAtPortOfExit = DateTimeMapper.Map(from?.EstimatedArrivalDateAtPortOfExit, from?.EstimatedArrivalTimeAtPortOfExit);
        return to;
    }
}