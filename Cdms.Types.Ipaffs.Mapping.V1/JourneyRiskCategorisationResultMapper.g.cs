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


namespace Cdms.Types.Ipaffs.Mapping;

public static class JourneyRiskCategorisationResultMapper
{
	public static Cdms.Model.Ipaffs.JourneyRiskCategorisationResult Map(Cdms.Types.Ipaffs.JourneyRiskCategorisationResult from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Cdms.Model.Ipaffs.JourneyRiskCategorisationResult ();
to.RiskLevel = JourneyRiskCategorisationResultRiskLevelEnumMapper.Map(from?.RiskLevel);
                to.RiskLevelMethod = JourneyRiskCategorisationResultRiskLevelMethodEnumMapper.Map(from?.RiskLevelMethod);
                to.RiskLevelDateTime = from.RiskLevelDateTime;
            	return to;
	}
}

