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

public static class UserInformationMapper
{
    public static Btms.Model.Ipaffs.UserInformation Map(Btms.Types.Ipaffs.UserInformation from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.UserInformation();
        to.DisplayName = from?.DisplayName;
        to.UserId = from?.UserId;
        to.IsControlUser = from?.IsControlUser;
        return to;
    }
}