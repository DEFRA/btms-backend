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
/// Result of risk assessment by the risk scorer
/// </summary>
public partial class RiskAssessmentResult  //
{


    /// <summary>
    /// List of risk assessed commodities
    /// </summary>
    [JsonPropertyName("commodityResults")]
    public CommodityRiskResult[]? CommodityResults { get; set; }


    /// <summary>
    /// Date and time of assessment
    /// </summary>
    [JsonPropertyName("assessmentDateTime")]
    [Btms.Common.Json.UnknownTimeZoneDateTimeJsonConverter(nameof(AssessmentDateTime))]
    public DateTime? AssessmentDateTime { get; set; }

}