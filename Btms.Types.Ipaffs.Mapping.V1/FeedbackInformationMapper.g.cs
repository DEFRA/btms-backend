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

public static class FeedbackInformationMapper
{
    public static Btms.Model.Ipaffs.FeedbackInformation Map(Btms.Types.Ipaffs.FeedbackInformation? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.FeedbackInformation();
        to.AuthorityType = FeedbackInformationAuthorityTypeEnumMapper.Map(from?.AuthorityType);
        to.ConsignmentArrival = from?.ConsignmentArrival;
        to.ConsignmentConformity = from?.ConsignmentConformity;
        to.ConsignmentNoArrivalReason = from?.ConsignmentNoArrivalReason;
        to.DestructionDate = from?.DestructionDate;
        return to;
    }
}