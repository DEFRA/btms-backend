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
/// PHSI Decision Breakdown
/// </summary>
public partial class Phsi  //
{


    /// <summary>
    /// Whether or not a documentary check is required for PHSI
    /// </summary>
    [JsonPropertyName("documentCheck")]
    public bool? DocumentCheck { get; set; }


    /// <summary>
    /// Whether or not an identity check is required for PHSI
    /// </summary>
    [JsonPropertyName("identityCheck")]
    public bool? IdentityCheck { get; set; }


    /// <summary>
    /// Whether or not a physical check is required for PHSI
    /// </summary>
    [JsonPropertyName("physicalCheck")]
    public bool? PhysicalCheck { get; set; }

}