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

public static class DecisionNotAcceptableActionEntryRefusalReasonEnumMapper
{
public static Btms.Model.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum? Map(Btms.Types.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum? from)
{
if(from == null)
{
return default!;
}
return from switch
{
Btms.Types.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.ContaminatedProducts => Btms.Model.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.ContaminatedProducts,
    Btms.Types.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.InterceptedPart => Btms.Model.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.InterceptedPart,
    Btms.Types.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.PackagingMaterial => Btms.Model.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.PackagingMaterial,
    Btms.Types.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.MeansOfTransport => Btms.Model.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.MeansOfTransport,
    Btms.Types.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.Other => Btms.Model.Ipaffs.DecisionNotAcceptableActionEntryRefusalReasonEnum.Other,

    _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
};
}
        

}


