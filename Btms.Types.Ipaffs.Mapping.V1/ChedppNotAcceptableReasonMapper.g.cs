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

public static class ChedppNotAcceptableReasonMapper
{
    public static Btms.Model.Ipaffs.ChedppNotAcceptableReason Map(Btms.Types.Ipaffs.ChedppNotAcceptableReason? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.ChedppNotAcceptableReason();
        to.Reason = ChedppNotAcceptableReasonReasonEnumMapper.Map(from?.Reason);
        to.CommodityOrPackage = ChedppNotAcceptableReasonCommodityOrPackageEnumMapper.Map(from?.CommodityOrPackage);
        return to;
    }
}