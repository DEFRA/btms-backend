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

public static class InspectionRequiredEnumMapper
{
    public static Btms.Model.Ipaffs.InspectionRequiredEnum? Map(Btms.Types.Ipaffs.InspectionRequiredEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.InspectionRequiredEnum.Required => Btms.Model.Ipaffs.InspectionRequiredEnum.Required,
            Btms.Types.Ipaffs.InspectionRequiredEnum.Inconclusive => Btms.Model.Ipaffs.InspectionRequiredEnum.Inconclusive,
            Btms.Types.Ipaffs.InspectionRequiredEnum.NotRequired => Btms.Model.Ipaffs.InspectionRequiredEnum.NotRequired,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}