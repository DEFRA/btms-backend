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
/// Details about the manual inspection override
/// </summary>
public partial class InspectionOverride  //
{


    /// <summary>
    /// Original inspection decision
    /// </summary>
    [JsonPropertyName("originalDecision")]
    public string? OriginalDecision { get; set; }


    /// <summary>
    /// The time the risk decision is overridden
    /// </summary>
    [JsonPropertyName("overriddenOn")]
    [Btms.Common.Json.UnknownTimeZoneDateTimeJsonConverter(nameof(OverriddenOn))]
    public DateTime? OverriddenOn { get; set; }


    /// <summary>
    /// User entity who has manually overridden the inspection
    /// </summary>
    [JsonPropertyName("overriddenBy")]
    public UserInformation? OverriddenBy { get; set; }

}