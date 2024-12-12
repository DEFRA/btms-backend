/*------------------------------------------------------------------------------
<auto-generated>
	This code was generated from the Mapper template.
	Manual changes to this file may cause unexpected behavior in your application.
	Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
namespace Btms.Types.Ipaffs.Mapping;

public static class EconomicOperatorMapper
{
	public static Btms.Model.Ipaffs.EconomicOperator? Map(Btms.Types.Ipaffs.EconomicOperator? from)
	{
		if (from == default) return default;

		var to = new Btms.Model.Ipaffs.EconomicOperator ();
        to.Id = from.Id;
        to.Type = EconomicOperatorTypeEnumMapper.Map(from.Type);
        to.Status = EconomicOperatorStatusEnumMapper.Map(from.Status);
        to.CompanyName = from.CompanyName;
        to.IndividualName = from.IndividualName;
        to.Address = AddressMapper.Map(from.Address);
        to.ApprovalNumber = from.ApprovalNumber;
        to.OtherIdentifier = from.OtherIdentifier;
        to.TracesId = from.TracesId;
		return to;
	}
}
