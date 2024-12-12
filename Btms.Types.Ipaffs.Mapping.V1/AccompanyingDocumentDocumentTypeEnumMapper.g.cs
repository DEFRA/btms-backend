/*------------------------------------------------------------------------------
<auto-generated>
    This code was generated from the EnumMapper template.
    Manual changes to this file may cause unexpected behavior in your application.
    Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
namespace Btms.Types.Ipaffs.Mapping;

public static class AccompanyingDocumentDocumentTypeEnumMapper
{
    public static Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum? Map(Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum? from)
    {
        if (from == default) return default;

        return from switch
        {
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.AirWaybill => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.AirWaybill,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.BillOfLading => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.BillOfLading,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CargoManifest => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CargoManifest,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CatchCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CatchCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CommercialDocument => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CommercialDocument,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CommercialInvoice => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CommercialInvoice,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ConformityCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ConformityCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ContainerManifest => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ContainerManifest,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CustomsDeclaration => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.CustomsDeclaration,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.Docom => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.Docom,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.HealthCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.HealthCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.HeatTreatmentCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.HeatTreatmentCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ImportPermit => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ImportPermit,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.InspectionCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.InspectionCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.Itahc => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.Itahc,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.JourneyLog => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.JourneyLog,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.LaboratorySamplingResultsForAflatoxin => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.LaboratorySamplingResultsForAflatoxin,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.LatestVeterinaryHealthCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.LatestVeterinaryHealthCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.LetterOfAuthority => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.LetterOfAuthority,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.LicenseOrAuthorisation => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.LicenseOrAuthorisation,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.MycotoxinCertification => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.MycotoxinCertification,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.OriginCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.OriginCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.Other => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.Other,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.PhytosanitaryCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.PhytosanitaryCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ProcessingStatement => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ProcessingStatement,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ProofOfStorage => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ProofOfStorage,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.RailwayBill => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.RailwayBill,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.SeaWaybill => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.SeaWaybill,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.VeterinaryHealthCertificate => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.VeterinaryHealthCertificate,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ListOfIngredients => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.ListOfIngredients,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.PackingList => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.PackingList,
            Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum.RoadConsignmentNote => Btms.Model.Ipaffs.AccompanyingDocumentDocumentTypeEnum.RoadConsignmentNote,
            _ => throw new ArgumentOutOfRangeException(nameof(from), from, "Unable to map enum AccompanyingDocumentDocumentTypeEnum value Btms.Types.Ipaffs.AccompanyingDocumentDocumentTypeEnum")
        };
    }
}
