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

public static class RouteMapper
{
    public static Btms.Model.Ipaffs.Route Map(Btms.Types.Ipaffs.Route from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.Route();
        to.TransitingStates = from?.TransitingStates;
        return to;
    }
}