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

public static class DecisionNotAcceptableActionSpecialTreatmentReasonEnumMapper
{
    public static Btms.Model.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum? Map(Btms.Types.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum.ContaminatedProducts => Btms.Model.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum.ContaminatedProducts,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum.InterceptedPart => Btms.Model.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum.InterceptedPart,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum.PackagingMaterial => Btms.Model.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum.PackagingMaterial,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum.Other => Btms.Model.Ipaffs.DecisionNotAcceptableActionSpecialTreatmentReasonEnum.Other,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}