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

public static class MeansOfTransportMapper
{
    public static Btms.Model.Ipaffs.MeansOfTransport Map(Btms.Types.Ipaffs.MeansOfTransport? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.MeansOfTransport();
        to.Type = MeansOfTransportTypeEnumMapper.Map(from?.Type);
        to.Document = from?.Document;
        to.Id = from?.Id;
        return to;
    }
}