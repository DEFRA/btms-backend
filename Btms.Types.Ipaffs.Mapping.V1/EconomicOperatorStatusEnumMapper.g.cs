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

public static class EconomicOperatorStatusEnumMapper
{
    public static Btms.Model.Ipaffs.EconomicOperatorStatusEnum? Map(Btms.Types.Ipaffs.EconomicOperatorStatusEnum? from)
    {
        if (from == null)
        {
            return default!;
        }

        return from switch
        {
            Btms.Types.Ipaffs.EconomicOperatorStatusEnum.Approved => Btms.Model.Ipaffs.EconomicOperatorStatusEnum
                .Approved,
            Btms.Types.Ipaffs.EconomicOperatorStatusEnum.Nonapproved => Btms.Model.Ipaffs.EconomicOperatorStatusEnum
                .Nonapproved,
            Btms.Types.Ipaffs.EconomicOperatorStatusEnum.Suspended => Btms.Model.Ipaffs.EconomicOperatorStatusEnum
                .Suspended,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}


