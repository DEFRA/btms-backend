//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

using System.Collections;
using System.Text.Json.Serialization;
using System.Dynamic;


namespace Btms.Types.Alvs;

/// <summary>
/// 
/// </summary>
public partial class Check  //
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("decisionCode")]
    public string? DecisionCode { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("decisionsValidUntil")]
    public DateTime? DecisionsValidUntil { get; set; }

    [JsonPropertyName("decisionReasons")]
    public string[]? DecisionReasons { get; set; }

}


