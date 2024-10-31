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

public static class CommodityComplementMapper
{
	public static Cdms.Model.Ipaffs.CommodityComplement Map(Cdms.Types.Ipaffs.CommodityComplement from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Cdms.Model.Ipaffs.CommodityComplement ();
to.UniqueComplementId = from.UniqueComplementId;
            to.CommodityDescription = from.CommodityDescription;
            to.CommodityId = from.CommodityId;
            to.ComplementId = from.ComplementId;
            to.ComplementName = from.ComplementName;
            to.EppoCode = from.EppoCode;
            to.IsWoodPackaging = from.IsWoodPackaging;
            to.SpeciesId = from.SpeciesId;
            to.SpeciesName = from.SpeciesName;
            to.SpeciesNomination = from.SpeciesNomination;
            to.SpeciesTypeName = from.SpeciesTypeName;
            to.SpeciesType = from.SpeciesType;
            to.SpeciesClassName = from.SpeciesClassName;
            to.SpeciesClass = from.SpeciesClass;
            to.SpeciesFamilyName = from.SpeciesFamilyName;
            to.SpeciesFamily = from.SpeciesFamily;
            to.SpeciesCommonName = from.SpeciesCommonName;
            to.IsCdsMatched = from.IsCdsMatched;
            to.AdditionalData = from.AdditionalData;
            to.RiskAssesment = CommodityRiskResultMapper.Map(from?.RiskAssesment);
                to.Checks = from?.Checks?.Select(x => InspectionCheckMapper.Map(x)).ToArray();
                	return to;
	}
}

