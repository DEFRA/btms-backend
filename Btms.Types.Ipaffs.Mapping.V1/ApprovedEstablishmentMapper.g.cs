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

public static class ApprovedEstablishmentMapper
{
    public static Btms.Model.Ipaffs.ApprovedEstablishment Map(Btms.Types.Ipaffs.ApprovedEstablishment? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.ApprovedEstablishment();
        to.Id = from?.Id;
        to.Name = from?.Name;
        to.Country = from?.Country;
        to.Types = from?.Types;
        to.ApprovalNumber = from?.ApprovalNumber;
        to.Section = from?.Section;
        return to;
    }
}