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

public static class SealContainerMapper
{
	public static Cdms.Model.Ipaffs.SealContainer Map(Cdms.Types.Ipaffs.SealContainer from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Cdms.Model.Ipaffs.SealContainer ();
to.SealNumber = from.SealNumber;
            to.ContainerNumber = from.ContainerNumber;
            to.OfficialSeal = from.OfficialSeal;
            to.ResealedSealNumber = from.ResealedSealNumber;
            	return to;
	}
}

