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

public static class ValidationMessageCodeMapper
{
    public static Btms.Model.Ipaffs.ValidationMessageCode Map(Btms.Types.Ipaffs.ValidationMessageCode from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.ValidationMessageCode();
        to.Field = from?.Field;
        to.Code = from?.Code;
        return to;
    }
}