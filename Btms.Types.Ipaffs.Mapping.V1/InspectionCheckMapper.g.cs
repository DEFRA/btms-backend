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

public static class InspectionCheckMapper
{
	public static Btms.Model.Ipaffs.InspectionCheck Map(Btms.Types.Ipaffs.InspectionCheck from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Btms.Model.Ipaffs.InspectionCheck ();
to.Type = InspectionCheckTypeEnumMapper.Map(from?.Type);
                to.Status = InspectionCheckStatusEnumMapper.Map(from?.Status);
                to.Reason = from?.Reason;
            to.OtherReason = from?.OtherReason;
            to.IsSelectedForChecks = from?.IsSelectedForChecks;
            to.HasChecksComplete = from?.HasChecksComplete;
            	return to;
	}
}

