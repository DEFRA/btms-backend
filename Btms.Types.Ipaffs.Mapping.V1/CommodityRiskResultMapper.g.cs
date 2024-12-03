//------------------------------------------------------------------------------
// <auto-generated>
	//     This code was generated from a template.
	//
	//     Manual changes to this file may cause unexpected behavior in your application.
	//     Manual changes to this file will be overwritten if the code is regenerated.
	//
//</auto-generated>
//------------------------------------------------------------------------------
#nullable enable


namespace Btms.Types.Ipaffs.Mapping;

public static class CommodityRiskResultMapper
{
    public static Btms.Model.Ipaffs.CommodityRiskResult Map(Btms.Types.Ipaffs.CommodityRiskResult from)
    {
        if (from is null)
        {
            return default!;
        }

        var to = new Btms.Model.Ipaffs.CommodityRiskResult();
        to.RiskDecision = CommodityRiskResultRiskDecisionEnumMapper.Map(from?.RiskDecision);
        to.ExitRiskDecision = CommodityRiskResultExitRiskDecisionEnumMapper.Map(from?.ExitRiskDecision);
        to.HmiDecision = CommodityRiskResultHmiDecisionEnumMapper.Map(from?.HmiDecision);
        to.PhsiDecision = CommodityRiskResultPhsiDecisionEnumMapper.Map(from?.PhsiDecision);
        to.PhsiClassification = CommodityRiskResultPhsiClassificationEnumMapper.Map(from?.PhsiClassification);
        to.Phsi = PhsiMapper.Map(from?.Phsi!);
        to.UniqueId = from?.UniqueId;
        to.EppoCode = from?.EppoCode;
        to.Variety = from?.Variety;
        to.IsWoody = from?.IsWoody;
        to.IndoorOutdoor = from?.IndoorOutdoor;
        to.Propagation = from?.Propagation;
        to.PhsiRuleType = from?.PhsiRuleType;
        return to;
    }
}

