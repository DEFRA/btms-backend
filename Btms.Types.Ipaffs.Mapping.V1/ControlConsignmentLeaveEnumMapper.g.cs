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

public static class ControlConsignmentLeaveEnumMapper
{
    public static Btms.Model.Ipaffs.ControlConsignmentLeaveEnum? Map(Btms.Types.Ipaffs.ControlConsignmentLeaveEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.ControlConsignmentLeaveEnum.Yes => Btms.Model.Ipaffs.ControlConsignmentLeaveEnum.Yes,
            Btms.Types.Ipaffs.ControlConsignmentLeaveEnum.No => Btms.Model.Ipaffs.ControlConsignmentLeaveEnum.No,
            Btms.Types.Ipaffs.ControlConsignmentLeaveEnum.ItHasBeenDestroyed => Btms.Model.Ipaffs.ControlConsignmentLeaveEnum.ItHasBeenDestroyed,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}