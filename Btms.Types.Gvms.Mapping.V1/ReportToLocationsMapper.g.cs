/*------------------------------------------------------------------------------
<auto-generated>
	This code was generated from the Mapper template.
	Manual changes to this file may cause unexpected behavior in your application.
	Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
namespace Btms.Types.Gvms.Mapping;

public static class ReportToLocationsMapper
{
	public static Btms.Model.Gvms.ReportToLocations? Map(Btms.Types.Gvms.ReportToLocations? from)
	{
		if (from == default) return default;

		var to = new Btms.Model.Gvms.ReportToLocations ();
        to.InspectionTypeId = from.InspectionTypeId;
		return to;
	}
}
