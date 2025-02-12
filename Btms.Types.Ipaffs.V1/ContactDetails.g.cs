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
using Btms.SensitiveData;


namespace Btms.Types.Ipaffs;

/// <summary>
/// Person to be contacted if there is an issue with the consignment
/// </summary>
public partial class ContactDetails  //
{


    /// <summary>
    /// Name of designated contact
    /// </summary>
    [JsonPropertyName("name")]
    [SensitiveData]
    public string? Name { get; set; }


    /// <summary>
    /// Telephone number of designated contact
    /// </summary>
    [JsonPropertyName("telephone")]
    [SensitiveData]
    public string? Telephone { get; set; }


    /// <summary>
    /// Email address of designated contact
    /// </summary>
    [JsonPropertyName("email")]
    [SensitiveData]
    public string? Email { get; set; }


    /// <summary>
    /// Name of agent representing designated contact
    /// </summary>
    [JsonPropertyName("agent")]
    [SensitiveData]
    public string? Agent { get; set; }

}