//------------------------------------------------------------------------------
// <auto-generated>
    //     This code was generated from a template.
    //
    //     Manual changes to this file may cause unexpected behavior in your application.
    //     Manual changes to this file will be overwritten if the code is regenerated.
    // </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
namespace Cdms.Types.Gmr.Mapping;

public static class HaulierTypeEnumMapper
{
public static Cdms.Model.VehicleMovement.HaulierTypeEnum? Map(Cdms.Types.Gmr.HaulierTypeEnum? from)
{
if(from == null)
{
return default!;
}
return from switch
{
Cdms.Types.Gmr.HaulierTypeEnum.Standard => Cdms.Model.VehicleMovement.HaulierTypeEnum.Standard,
    Cdms.Types.Gmr.HaulierTypeEnum.FpoAsn => Cdms.Model.VehicleMovement.HaulierTypeEnum.FpoAsn,
    Cdms.Types.Gmr.HaulierTypeEnum.FpoOther => Cdms.Model.VehicleMovement.HaulierTypeEnum.FpoOther,
    Cdms.Types.Gmr.HaulierTypeEnum.NatoMod => Cdms.Model.VehicleMovement.HaulierTypeEnum.NatoMod,
    Cdms.Types.Gmr.HaulierTypeEnum.Rmg => Cdms.Model.VehicleMovement.HaulierTypeEnum.Rmg,
    Cdms.Types.Gmr.HaulierTypeEnum.Etoe => Cdms.Model.VehicleMovement.HaulierTypeEnum.Etoe,
     
};
}
        

}


