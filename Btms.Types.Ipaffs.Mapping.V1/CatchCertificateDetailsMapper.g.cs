/*------------------------------------------------------------------------------
<auto-generated>
	This code was generated from the Mapper template.
	Manual changes to this file may cause unexpected behavior in your application.
	Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
namespace Btms.Types.Ipaffs.Mapping;

public static class CatchCertificateDetailsMapper
{
	public static Btms.Model.Ipaffs.CatchCertificateDetails? Map(Btms.Types.Ipaffs.CatchCertificateDetails? from)
	{
		if (from == default) return default;

		var to = new Btms.Model.Ipaffs.CatchCertificateDetails ();
        to.CatchCertificateId = from.CatchCertificateId;
        to.CatchCertificateReference = from.CatchCertificateReference;
        to.IssuedOn = from.DateOfIssue;
        to.FlagState = from.FlagState;
        to.Species = from.Species;
		return to;
	}
}
