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

public static class ConsignmentCheckIdentityCheckTypeEnumMapper
{
    public static Btms.Model.Ipaffs.ConsignmentCheckIdentityCheckTypeEnum? Map(Btms.Types.Ipaffs.ConsignmentCheckIdentityCheckTypeEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.ConsignmentCheckIdentityCheckTypeEnum.SealCheck => Btms.Model.Ipaffs.ConsignmentCheckIdentityCheckTypeEnum.SealCheck,
            Btms.Types.Ipaffs.ConsignmentCheckIdentityCheckTypeEnum.FullIdentityCheck => Btms.Model.Ipaffs.ConsignmentCheckIdentityCheckTypeEnum.FullIdentityCheck,
            Btms.Types.Ipaffs.ConsignmentCheckIdentityCheckTypeEnum.NotDone => Btms.Model.Ipaffs.ConsignmentCheckIdentityCheckTypeEnum.NotDone,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}