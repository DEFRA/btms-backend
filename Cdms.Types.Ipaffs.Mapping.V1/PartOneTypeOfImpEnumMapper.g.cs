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

public static class PartOneTypeOfImpEnumMapper
{
public static Cdms.Model.Ipaffs.PartOneTypeOfImpEnum? Map(Cdms.Types.Ipaffs.PartOneTypeOfImpEnum? from)
{
if(from == null)
{
return default!;
}
return from switch
{
Cdms.Types.Ipaffs.PartOneTypeOfImpEnum.A => Cdms.Model.Ipaffs.PartOneTypeOfImpEnum.A,
    Cdms.Types.Ipaffs.PartOneTypeOfImpEnum.P => Cdms.Model.Ipaffs.PartOneTypeOfImpEnum.P,
    Cdms.Types.Ipaffs.PartOneTypeOfImpEnum.D => Cdms.Model.Ipaffs.PartOneTypeOfImpEnum.D,
     
};
}
        

}


