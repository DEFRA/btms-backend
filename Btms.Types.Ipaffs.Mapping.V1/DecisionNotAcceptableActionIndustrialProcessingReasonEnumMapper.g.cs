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

public static class DecisionNotAcceptableActionIndustrialProcessingReasonEnumMapper
{
public static Btms.Model.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum? Map(Btms.Types.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum? from)
{
if(from == null)
{
return default!;
}
return from switch
{
Btms.Types.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum.ContaminatedProducts => Btms.Model.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum.ContaminatedProducts,
    Btms.Types.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum.InterceptedPart => Btms.Model.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum.InterceptedPart,
    Btms.Types.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum.PackagingMaterial => Btms.Model.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum.PackagingMaterial,
    Btms.Types.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum.Other => Btms.Model.Ipaffs.DecisionNotAcceptableActionIndustrialProcessingReasonEnum.Other,

    _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
};
}
        

}


