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

public static class DecisionNotAcceptableActionEnumMapper
{
    public static Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum? Map(Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.Slaughter => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.Slaughter,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.Reexport => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.Reexport,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.Euthanasia => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.Euthanasia,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.Redispatching => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.Redispatching,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.Destruction => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.Destruction,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.Transformation => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.Transformation,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.Other => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.Other,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.EntryRefusal => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.EntryRefusal,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.QuarantineImposed => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.QuarantineImposed,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.SpecialTreatment => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.SpecialTreatment,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.IndustrialProcessing => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.IndustrialProcessing,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.ReDispatch => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.ReDispatch,
            Btms.Types.Ipaffs.DecisionNotAcceptableActionEnum.UseForOtherPurposes => Btms.Model.Ipaffs.DecisionNotAcceptableActionEnum.UseForOtherPurposes,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}