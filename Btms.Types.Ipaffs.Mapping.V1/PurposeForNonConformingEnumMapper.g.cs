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

public static class PurposeForNonConformingEnumMapper
{
    public static Btms.Model.Ipaffs.PurposeForNonConformingEnum? Map(
        Btms.Types.Ipaffs.PurposeForNonConformingEnum? from)
    {
        if (from == null)
        {
            return default!;
        }

        return from switch
        {
            Btms.Types.Ipaffs.PurposeForNonConformingEnum.CustomsWarehouse => Btms.Model.Ipaffs
                .PurposeForNonConformingEnum.CustomsWarehouse,
            Btms.Types.Ipaffs.PurposeForNonConformingEnum.FreeZoneOrFreeWarehouse => Btms.Model.Ipaffs
                .PurposeForNonConformingEnum.FreeZoneOrFreeWarehouse,
            Btms.Types.Ipaffs.PurposeForNonConformingEnum.ShipSupplier => Btms.Model.Ipaffs.PurposeForNonConformingEnum
                .ShipSupplier,
            Btms.Types.Ipaffs.PurposeForNonConformingEnum.Ship => Btms.Model.Ipaffs.PurposeForNonConformingEnum.Ship,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}


