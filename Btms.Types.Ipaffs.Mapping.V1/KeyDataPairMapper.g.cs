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

public static class KeyDataPairMapper
{
	public static Btms.Model.Ipaffs.KeyDataPair Map(Btms.Types.Ipaffs.KeyDataPair from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Btms.Model.Ipaffs.KeyDataPair ();
to.Key = from?.Key;
            to.Data = from?.Data;
            	return to;
	}
}

