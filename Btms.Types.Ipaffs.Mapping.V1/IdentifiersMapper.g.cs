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

public static class IdentifiersMapper
{
    public static Btms.Model.Ipaffs.Identifiers Map(Btms.Types.Ipaffs.Identifiers? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.Identifiers();
        to.SpeciesNumber = from?.SpeciesNumber;
        to.Data = from?.Data;
        to.IsPlaceOfDestinationThePermanentAddress = from?.IsPlaceOfDestinationThePermanentAddress;
        to.PermanentAddress = EconomicOperatorMapper.Map(from?.PermanentAddress);
        return to;
    }
}