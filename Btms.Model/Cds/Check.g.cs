//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable

using JsonApiDotNetCore.Resources.Annotations;
using System.Text.Json.Serialization;
using System.Dynamic;


namespace Btms.Model.Cds;

/// <summary>
/// 
/// </summary>
public partial class Check  //
{

    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? CheckCode { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? DepartmentCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [JsonPropertyName("decisionCode")]
    public string? DecisionCode { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [JsonPropertyName("decisionsValidUntil")]
    public DateTime? DecisionsValidUntil { get; set; }

    [Attr]
    [JsonPropertyName("decisionReasons")]
    public string[]? DecisionReasons { get; set; }

}


