//------------------------------------------------------------------------------
// <auto-generated>
    //     This code was generated from a template.
    //
    //     Manual changes to this file may cause unexpected behavior in your application.
    //     Manual changes to this file will be overwritten if the code is regenerated.
    // </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
namespace Btms.Types.Gvms.Mapping;

public static class HaulierTypeEnumMapper
{
public static Btms.Model.Gvms.HaulierTypeEnum? Map(Btms.Types.Gvms.HaulierTypeEnum? from)
{
if(from == null)
{
return default!;
}
return from switch
{
Btms.Types.Gvms.HaulierTypeEnum.Standard => Btms.Model.Gvms.HaulierTypeEnum.Standard,
    Btms.Types.Gvms.HaulierTypeEnum.FpoAsn => Btms.Model.Gvms.HaulierTypeEnum.FpoAsn,
    Btms.Types.Gvms.HaulierTypeEnum.FpoOther => Btms.Model.Gvms.HaulierTypeEnum.FpoOther,
    Btms.Types.Gvms.HaulierTypeEnum.NatoMod => Btms.Model.Gvms.HaulierTypeEnum.NatoMod,
    Btms.Types.Gvms.HaulierTypeEnum.Rmg => Btms.Model.Gvms.HaulierTypeEnum.Rmg,
    Btms.Types.Gvms.HaulierTypeEnum.Etoe => Btms.Model.Gvms.HaulierTypeEnum.Etoe,
    
_ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
};
}
        

}


