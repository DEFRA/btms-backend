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

public static class PartOneProvideCtcMrnEnumMapper
{
    public static Btms.Model.Ipaffs.PartOneProvideCtcMrnEnum? Map(Btms.Types.Ipaffs.PartOneProvideCtcMrnEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.PartOneProvideCtcMrnEnum.Yes => Btms.Model.Ipaffs.PartOneProvideCtcMrnEnum.Yes,
            Btms.Types.Ipaffs.PartOneProvideCtcMrnEnum.YesAddLater => Btms.Model.Ipaffs.PartOneProvideCtcMrnEnum.YesAddLater,
            Btms.Types.Ipaffs.PartOneProvideCtcMrnEnum.No => Btms.Model.Ipaffs.PartOneProvideCtcMrnEnum.No,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}