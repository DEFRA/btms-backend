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
/// Details of transport
/// </summary>
public partial class MeansOfTransport  //
{


    /// <summary>
    /// Type of transport
    /// </summary>
    [JsonPropertyName("type")]
    public MeansOfTransportTypeEnum? Type { get; set; }


    /// <summary>
    /// Document for transport
    /// </summary>
    [JsonPropertyName("document")]
    [Btms.SensitiveData.SensitiveData]
    public string? Document { get; set; }


    /// <summary>
    /// ID of transport
    /// </summary>
    [JsonPropertyName("id")]
    [Btms.SensitiveData.SensitiveData]
    public string? Id { get; set; }

}