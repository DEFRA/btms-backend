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
/// Seal container details
/// </summary>
public partial class SealContainer  //
{


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("sealNumber")]
    [Btms.SensitiveData.SensitiveData]
    public string? SealNumber { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("containerNumber")]
    [Btms.SensitiveData.SensitiveData]
    public string? ContainerNumber { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("officialSeal")]
    public bool? OfficialSeal { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("resealedSealNumber")]
    [Btms.SensitiveData.SensitiveData]
    public string? ResealedSealNumber { get; set; }

}