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

public static class OfficialVeterinarianMapper
{
	public static Btms.Model.Ipaffs.OfficialVeterinarian Map(Btms.Types.Ipaffs.OfficialVeterinarian from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Btms.Model.Ipaffs.OfficialVeterinarian ();
to.FirstName = from?.FirstName;
            to.LastName = from?.LastName;
            to.Email = from?.Email;
            to.Phone = from?.Phone;
            to.Fax = from?.Fax;
            to.Signed = from?.Signed;
            	return to;
	}
}

