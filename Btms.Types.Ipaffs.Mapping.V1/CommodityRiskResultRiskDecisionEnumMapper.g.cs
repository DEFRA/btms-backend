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

public static class CommodityRiskResultRiskDecisionEnumMapper
{
public static Btms.Model.Ipaffs.CommodityRiskResultRiskDecisionEnum? Map(Btms.Types.Ipaffs.CommodityRiskResultRiskDecisionEnum? from)
{
if(from == null)
{
return default!;
}
return from switch
{
Btms.Types.Ipaffs.CommodityRiskResultRiskDecisionEnum.Required => Btms.Model.Ipaffs.CommodityRiskResultRiskDecisionEnum.Required,
    Btms.Types.Ipaffs.CommodityRiskResultRiskDecisionEnum.Notrequired => Btms.Model.Ipaffs.CommodityRiskResultRiskDecisionEnum.Notrequired,
    Btms.Types.Ipaffs.CommodityRiskResultRiskDecisionEnum.Inconclusive => Btms.Model.Ipaffs.CommodityRiskResultRiskDecisionEnum.Inconclusive,
    Btms.Types.Ipaffs.CommodityRiskResultRiskDecisionEnum.ReenforcedCheck => Btms.Model.Ipaffs.CommodityRiskResultRiskDecisionEnum.ReenforcedCheck,

    _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
};
}
        

}


