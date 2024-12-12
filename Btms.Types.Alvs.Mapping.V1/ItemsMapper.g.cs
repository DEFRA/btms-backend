/*------------------------------------------------------------------------------
<auto-generated>
	This code was generated from the Mapper template.
	Manual changes to this file may cause unexpected behavior in your application.
	Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
namespace Btms.Types.Alvs.Mapping;

public static class ItemsMapper
{
	public static Btms.Model.Alvs.Items? Map(Btms.Types.Alvs.Items? from)
	{
		if (from == default) return default;

		var to = new Btms.Model.Alvs.Items ();
        to.ItemNumber = from.ItemNumber;
        to.CustomsProcedureCode = from.CustomsProcedureCode;
        to.TaricCommodityCode = from.TaricCommodityCode;
        to.GoodsDescription = from.GoodsDescription;
        to.ConsigneeId = from.ConsigneeId;
        to.ConsigneeName = from.ConsigneeName;
        to.ItemNetMass = from.ItemNetMass;
        to.ItemSupplementaryUnits = from.ItemSupplementaryUnits;
        to.ItemThirdQuantity = from.ItemThirdQuantity;
        to.ItemOriginCountryCode = from.ItemOriginCountryCode;
        to.Documents = from.Documents?.Select(x => DocumentMapper.Map(x)).Where(x => x != null).ToArray()!;
        to.Checks = from.Checks?.Select(x => CheckMapper.Map(x)).Where(x => x != null).ToArray()!;
		return to;
	}
}
