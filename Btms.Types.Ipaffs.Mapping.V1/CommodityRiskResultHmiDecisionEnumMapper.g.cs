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

public static class CommodityRiskResultHmiDecisionEnumMapper
{
public static Btms.Model.Ipaffs.CommodityRiskResultHmiDecisionEnum? Map(Btms.Types.Ipaffs.CommodityRiskResultHmiDecisionEnum? from)
{
if(from == null)
{
return default!;
}
return from switch
{
Btms.Types.Ipaffs.CommodityRiskResultHmiDecisionEnum.Required => Btms.Model.Ipaffs.CommodityRiskResultHmiDecisionEnum.Required,
    Btms.Types.Ipaffs.CommodityRiskResultHmiDecisionEnum.Notrequired => Btms.Model.Ipaffs.CommodityRiskResultHmiDecisionEnum.Notrequired,

    _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
};
}
        

}


