//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
namespace Btms.Types.Ipaffs.Mapping;

public static class FeedbackInformationAuthorityTypeEnumMapper
{
    public static Btms.Model.Ipaffs.FeedbackInformationAuthorityTypeEnum? Map(Btms.Types.Ipaffs.FeedbackInformationAuthorityTypeEnum? from)
    {
        if (from == null)
        {
            return default!;
        }
        return from switch
        {
            Btms.Types.Ipaffs.FeedbackInformationAuthorityTypeEnum.Exitbip => Btms.Model.Ipaffs.FeedbackInformationAuthorityTypeEnum.Exitbip,
            Btms.Types.Ipaffs.FeedbackInformationAuthorityTypeEnum.Finalbip => Btms.Model.Ipaffs.FeedbackInformationAuthorityTypeEnum.Finalbip,
            Btms.Types.Ipaffs.FeedbackInformationAuthorityTypeEnum.Localvetunit => Btms.Model.Ipaffs.FeedbackInformationAuthorityTypeEnum.Localvetunit,
            Btms.Types.Ipaffs.FeedbackInformationAuthorityTypeEnum.Inspunit => Btms.Model.Ipaffs.FeedbackInformationAuthorityTypeEnum.Inspunit,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}