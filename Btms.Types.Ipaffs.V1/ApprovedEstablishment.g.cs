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
/// Approved Establishment details
/// </summary>
public partial class ApprovedEstablishment  //
{


    /// <summary>
    /// ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }


    /// <summary>
    /// Name of approved establishment
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }


    /// <summary>
    /// Country of approved establishment
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }


    /// <summary>
    /// Types of approved establishment
    /// </summary>
    [JsonPropertyName("types")]
    public string[]? Types { get; set; }


    /// <summary>
    /// Approval number
    /// </summary>
    [JsonPropertyName("approvalNumber")]
    public string? ApprovalNumber { get; set; }


    /// <summary>
    /// Section of approved establishment
    /// </summary>
    [JsonPropertyName("section")]
    public string? Section { get; set; }

}