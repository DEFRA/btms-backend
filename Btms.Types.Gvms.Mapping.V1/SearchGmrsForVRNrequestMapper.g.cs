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


namespace Btms.Types.Gvms.Mapping;

public static class SearchGmrsForVRNrequestMapper
{
    public static Btms.Model.Gvms.SearchGmrsForVRNrequest Map(Btms.Types.Gvms.SearchGmrsForVRNrequest? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Gvms.SearchGmrsForVRNrequest();
        to.Vrns = from?.Vrns;
        return to;
    }
}