/*------------------------------------------------------------------------------
<auto-generated>
	This code was generated from the Mapper template.
	Manual changes to this file may cause unexpected behavior in your application.
	Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
namespace Btms.Types.Ipaffs.Mapping;

public static class PartThreeMapper
{
	public static Btms.Model.Ipaffs.PartThree? Map(Btms.Types.Ipaffs.PartThree? from)
	{
		if (from == default) return default;

		var to = new Btms.Model.Ipaffs.PartThree ();
        to.ControlStatus = PartThreeControlStatusEnumMapper.Map(from.ControlStatus);
        to.Control = ControlMapper.Map(from.Control);
        to.ConsignmentValidations = from.ConsignmentValidations?.Select(x => ValidationMessageCodeMapper.Map(x)).Where(x => x != null).ToArray()!;
        to.SealCheckRequired = from.SealCheckRequired;
        to.SealCheck = SealCheckMapper.Map(from.SealCheck);
        to.SealCheckOverride = InspectionOverrideMapper.Map(from.SealCheckOverride);
		return to;
	}
}
