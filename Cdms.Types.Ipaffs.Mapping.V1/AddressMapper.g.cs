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

public static class AddressMapper
{
	public static Cdms.Model.Ipaffs.Address Map(Cdms.Types.Ipaffs.Address from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Cdms.Model.Ipaffs.Address ();
to.Street = from.Street;
            to.City = from.City;
            to.Country = from.Country;
            to.PostalCode = from.PostalCode;
            to.AddressLine1 = from.AddressLine1;
            to.AddressLine2 = from.AddressLine2;
            to.AddressLine3 = from.AddressLine3;
            to.PostalZipCode = from.PostalZipCode;
            to.CountryIsoCode = from.CountryIsoCode;
            to.Email = from.Email;
            to.UkTelephone = from.UkTelephone;
            to.Telephone = from.Telephone;
            to.InternationalTelephone = InternationalTelephoneMapper.Map(from?.InternationalTelephone);
                	return to;
	}
}

