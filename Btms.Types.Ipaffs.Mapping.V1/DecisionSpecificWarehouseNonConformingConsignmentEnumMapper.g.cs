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

public static class DecisionSpecificWarehouseNonConformingConsignmentEnumMapper
{
    public static Btms.Model.Ipaffs.DecisionSpecificWarehouseNonConformingConsignmentEnum? Map(
        Btms.Types.Ipaffs.DecisionSpecificWarehouseNonConformingConsignmentEnum? from)
    {
        if (from == null)
        {
            return default!;
        }

        return from switch
        {
            Btms.Types.Ipaffs.DecisionSpecificWarehouseNonConformingConsignmentEnum.CustomWarehouse => Btms.Model.Ipaffs
                .DecisionSpecificWarehouseNonConformingConsignmentEnum.CustomWarehouse,
            Btms.Types.Ipaffs.DecisionSpecificWarehouseNonConformingConsignmentEnum.FreeZoneOrFreeWarehouse => Btms.Model.Ipaffs.DecisionSpecificWarehouseNonConformingConsignmentEnum.FreeZoneOrFreeWarehouse,
            Btms.Types.Ipaffs.DecisionSpecificWarehouseNonConformingConsignmentEnum.ShipSupplier => Btms.Model.Ipaffs
                .DecisionSpecificWarehouseNonConformingConsignmentEnum.ShipSupplier,
            Btms.Types.Ipaffs.DecisionSpecificWarehouseNonConformingConsignmentEnum.Ship => Btms.Model.Ipaffs
                .DecisionSpecificWarehouseNonConformingConsignmentEnum.Ship,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}