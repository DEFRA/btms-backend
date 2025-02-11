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
/// inspector
/// </summary>
public partial class Inspector  //
{


    /// <summary>
    /// Name of inspector
    /// </summary>
    [JsonPropertyName("name")]
    [Btms.SensitiveData.SensitiveData]
    public string? Name { get; set; }


    /// <summary>
    /// Phone number of inspector
    /// </summary>
    [JsonPropertyName("phone")]
    [Btms.SensitiveData.SensitiveData]
    public string? Phone { get; set; }


    /// <summary>
    /// Email address of inspector
    /// </summary>
    [JsonPropertyName("email")]
    [Btms.SensitiveData.SensitiveData]
    public string? Email { get; set; }

}