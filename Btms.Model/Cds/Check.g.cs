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
    [System.ComponentModel.Description("")]
    public string? DecisionCode { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [JsonPropertyName("decisionsValidUntil")]
    [System.ComponentModel.Description("")]
    public DateTime? DecisionsValidUntil { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [JsonPropertyName("decisionReasons")]
    [System.ComponentModel.Description("")]
    public string[]? DecisionReasons { get; set; }

}