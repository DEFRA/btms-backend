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

public static class DecisionIfChanneledOptionEnumMapper
{
    public static Btms.Model.Ipaffs.DecisionIfChanneledOptionEnum? Map(Btms.Types.Ipaffs.DecisionIfChanneledOptionEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.DecisionIfChanneledOptionEnum.Article8 => Btms.Model.Ipaffs.DecisionIfChanneledOptionEnum.Article8,
            Btms.Types.Ipaffs.DecisionIfChanneledOptionEnum.Article15 => Btms.Model.Ipaffs.DecisionIfChanneledOptionEnum.Article15,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}