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

public static class InspectionCheckStatusEnumMapper
{
    public static Btms.Model.Ipaffs.InspectionCheckStatusEnum? Map(Btms.Types.Ipaffs.InspectionCheckStatusEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.InspectionCheckStatusEnum.ToDo => Btms.Model.Ipaffs.InspectionCheckStatusEnum.ToDo,
            Btms.Types.Ipaffs.InspectionCheckStatusEnum.Compliant => Btms.Model.Ipaffs.InspectionCheckStatusEnum.Compliant,
            Btms.Types.Ipaffs.InspectionCheckStatusEnum.AutoCleared => Btms.Model.Ipaffs.InspectionCheckStatusEnum.AutoCleared,
            Btms.Types.Ipaffs.InspectionCheckStatusEnum.NonCompliant => Btms.Model.Ipaffs.InspectionCheckStatusEnum.NonCompliant,
            Btms.Types.Ipaffs.InspectionCheckStatusEnum.NotInspected => Btms.Model.Ipaffs.InspectionCheckStatusEnum.NotInspected,
            Btms.Types.Ipaffs.InspectionCheckStatusEnum.ToBeInspected => Btms.Model.Ipaffs.InspectionCheckStatusEnum.ToBeInspected,
            Btms.Types.Ipaffs.InspectionCheckStatusEnum.Hold => Btms.Model.Ipaffs.InspectionCheckStatusEnum.Hold,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}