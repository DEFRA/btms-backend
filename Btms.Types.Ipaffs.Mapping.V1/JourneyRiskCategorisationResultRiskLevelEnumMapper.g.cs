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

public static class JourneyRiskCategorisationResultRiskLevelEnumMapper
{
    public static Btms.Model.Ipaffs.JourneyRiskCategorisationResultRiskLevelEnum? Map(Btms.Types.Ipaffs.JourneyRiskCategorisationResultRiskLevelEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.JourneyRiskCategorisationResultRiskLevelEnum.High => Btms.Model.Ipaffs.JourneyRiskCategorisationResultRiskLevelEnum.High,
            Btms.Types.Ipaffs.JourneyRiskCategorisationResultRiskLevelEnum.Medium => Btms.Model.Ipaffs.JourneyRiskCategorisationResultRiskLevelEnum.Medium,
            Btms.Types.Ipaffs.JourneyRiskCategorisationResultRiskLevelEnum.Low => Btms.Model.Ipaffs.JourneyRiskCategorisationResultRiskLevelEnum.Low,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}