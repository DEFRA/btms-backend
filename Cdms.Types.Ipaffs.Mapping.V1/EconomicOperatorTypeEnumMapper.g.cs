//------------------------------------------------------------------------------
// <auto-generated>
    //     This code was generated from a template.
    //
    //     Manual changes to this file may cause unexpected behavior in your application.
    //     Manual changes to this file will be overwritten if the code is regenerated.
    // </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
namespace Cdms.Types.Ipaffs.Mapping;

public static class EconomicOperatorTypeEnumMapper
{
public static Cdms.Model.Ipaffs.EconomicOperatorTypeEnum? Map(Cdms.Types.Ipaffs.EconomicOperatorTypeEnum? from)
{
if(from == null)
{
return default!;
}
return from switch
{
Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.Consignee => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.Consignee,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.Destination => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.Destination,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.Exporter => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.Exporter,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.Importer => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.Importer,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.Charity => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.Charity,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.CommercialTransporter => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.CommercialTransporter,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.CommercialTransporterUserAdded => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.CommercialTransporterUserAdded,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.PrivateTransporter => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.PrivateTransporter,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.TemporaryAddress => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.TemporaryAddress,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.PremisesOfOrigin => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.PremisesOfOrigin,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.OrganisationBranchAddress => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.OrganisationBranchAddress,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.Packer => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.Packer,
    Cdms.Types.Ipaffs.EconomicOperatorTypeEnum.Pod => Cdms.Model.Ipaffs.EconomicOperatorTypeEnum.Pod,
     
};
}
        

}


