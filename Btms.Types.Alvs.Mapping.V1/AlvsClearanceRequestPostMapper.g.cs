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


namespace Btms.Types.Alvs.Mapping;

public static class AlvsClearanceRequestPostMapper
{
    public static Btms.Model.Cds.CdsClearanceRequestPost Map(Btms.Types.Alvs.AlvsClearanceRequestPost from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Cds.CdsClearanceRequestPost();
        to.XmlSchemaVersion = from.XmlSchemaVersion;
        to.UserIdentification = from.UserIdentification;
        to.UserPassword = from.UserPassword;
        to.SentOn = from.SendingDate;
        to.AlvsClearanceRequest = AlvsClearanceRequestMapper.Map(from.AlvsClearanceRequest!);
        return to;
    }
}