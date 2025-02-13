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

public static class CatchCertificateAttachmentMapper
{
    public static Btms.Model.Ipaffs.CatchCertificateAttachment Map(Btms.Types.Ipaffs.CatchCertificateAttachment from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.CatchCertificateAttachment();
        to.AttachmentId = from?.AttachmentId;
        to.NumberOfCatchCertificates = from?.NumberOfCatchCertificates;
        to.CatchCertificateDetails = from?.CatchCertificateDetails?.Select(x => CatchCertificateDetailsMapper.Map(x)).ToArray();
        return to;
    }
}