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

public static class PostalAddressMapper
{
    public static Btms.Model.Ipaffs.PostalAddress Map(Btms.Types.Ipaffs.PostalAddress? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.PostalAddress();
        to.AddressLine1 = from?.AddressLine1;
        to.AddressLine2 = from?.AddressLine2;
        to.AddressLine3 = from?.AddressLine3;
        to.AddressLine4 = from?.AddressLine4;
        to.County = from?.County;
        to.CityOrTown = from?.CityOrTown;
        to.PostalCode = from?.PostalCode;
        return to;
    }
}