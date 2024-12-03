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
/// 
/// </summary>
public partial class InspectionCheck  //
{


        /// <summary>
        /// Type of check
        /// </summary>
    [JsonPropertyName("type")]
    public InspectionCheckTypeEnum? Type { get; set; }

	
        /// <summary>
        /// Status of the check
        /// </summary>
    [JsonPropertyName("status")]
    public InspectionCheckStatusEnum? Status { get; set; }

	
        /// <summary>
        /// Reason for the status if applicable
        /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

	
        /// <summary>
        /// Other reason text when selected reason is &#x27;Other&#x27;
        /// </summary>
    [JsonPropertyName("otherReason")]
    public string? OtherReason { get; set; }

	
        /// <summary>
        /// Has commodity been selected for checks?
        /// </summary>
    [JsonPropertyName("isSelectedForChecks")]
    public bool? IsSelectedForChecks { get; set; }

	
        /// <summary>
        /// Has commodity completed this type of check
        /// </summary>
    [JsonPropertyName("hasChecksComplete")]
    public bool? HasChecksComplete { get; set; }

	}


