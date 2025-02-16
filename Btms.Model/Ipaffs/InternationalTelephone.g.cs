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


namespace Btms.Model.Ipaffs;

/// <summary>
/// International phone number
/// </summary>
public partial class InternationalTelephone  //
{


    /// <summary>
    /// Country code of phone number
    /// </summary>
    [Attr]
    [JsonPropertyName("countryCode")]
    [System.ComponentModel.Description("Country code of phone number")]
    public string? CountryCode { get; set; }


    /// <summary>
    /// Phone number
    /// </summary>
    [Attr]
    [JsonPropertyName("subscriberNumber")]
    [System.ComponentModel.Description("Phone number")]
    public string? SubscriberNumber { get; set; }

}