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

public static class ApplicantAnalysisTypeEnumMapper
{
    public static Btms.Model.Ipaffs.ApplicantAnalysisTypeEnum? Map(Btms.Types.Ipaffs.ApplicantAnalysisTypeEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.ApplicantAnalysisTypeEnum.InitialAnalysis => Btms.Model.Ipaffs.ApplicantAnalysisTypeEnum.InitialAnalysis,
            Btms.Types.Ipaffs.ApplicantAnalysisTypeEnum.CounterAnalysis => Btms.Model.Ipaffs.ApplicantAnalysisTypeEnum.CounterAnalysis,
            Btms.Types.Ipaffs.ApplicantAnalysisTypeEnum.SecondExpertAnalysis => Btms.Model.Ipaffs.ApplicantAnalysisTypeEnum.SecondExpertAnalysis,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}