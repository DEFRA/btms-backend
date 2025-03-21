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

public static class ExternalReferenceMapper
{
    public static Btms.Model.Ipaffs.ExternalReference Map(Btms.Types.Ipaffs.ExternalReference? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.ExternalReference();
        to.System = ExternalReferenceSystemEnumMapper.Map(from?.System);
        to.Reference = from?.Reference;
        to.ExactMatch = from?.ExactMatch;
        to.VerifiedByImporter = from?.VerifiedByImporter;
        to.VerifiedByInspector = from?.VerifiedByInspector;
        return to;
    }
}