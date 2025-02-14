//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

using System.Text.Json.Serialization;
using System.Dynamic;


namespace Btms.Types.Ipaffs;


/// <summary>
/// Feedback information from control
/// </summary>
public partial class FeedbackInformation  //
{


    /// <summary>
    /// Type of authority
    /// </summary>
    [JsonPropertyName("authorityType")]
    public FeedbackInformationAuthorityTypeEnum? AuthorityType { get; set; }


    /// <summary>
    /// Did the consignment arrive
    /// </summary>
    [JsonPropertyName("consignmentArrival")]
    public bool? ConsignmentArrival { get; set; }


    /// <summary>
    /// Does the consignment conform
    /// </summary>
    [JsonPropertyName("consignmentConformity")]
    public bool? ConsignmentConformity { get; set; }


    /// <summary>
    /// Reason for consignment not arriving at the entry point
    /// </summary>
    [JsonPropertyName("consignmentNoArrivalReason")]
    public string? ConsignmentNoArrivalReason { get; set; }


    /// <summary>
    /// Date of consignment destruction
    /// </summary>
    [JsonPropertyName("destructionDate")]
    public string? DestructionDate { get; set; }

}