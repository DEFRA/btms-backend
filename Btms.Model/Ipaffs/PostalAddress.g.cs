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
/// Billing postal address
/// </summary>
public partial class PostalAddress  //
{


    /// <summary>
    /// 1st line of address
    /// </summary>
    [Attr]
    [JsonPropertyName("addressLine1")]
    [System.ComponentModel.Description("1st line of address")]
    public string? AddressLine1 { get; set; }


    /// <summary>
    /// 2nd line of address
    /// </summary>
    [Attr]
    [JsonPropertyName("addressLine2")]
    [System.ComponentModel.Description("2nd line of address")]
    public string? AddressLine2 { get; set; }


    /// <summary>
    /// 3rd line of address
    /// </summary>
    [Attr]
    [JsonPropertyName("addressLine3")]
    [System.ComponentModel.Description("3rd line of address")]
    public string? AddressLine3 { get; set; }


    /// <summary>
    /// 4th line of address
    /// </summary>
    [Attr]
    [JsonPropertyName("addressLine4")]
    [System.ComponentModel.Description("4th line of address")]
    public string? AddressLine4 { get; set; }


    /// <summary>
    /// 3rd line of address
    /// </summary>
    [Attr]
    [JsonPropertyName("county")]
    [System.ComponentModel.Description("3rd line of address")]
    public string? County { get; set; }


    /// <summary>
    /// City or town name
    /// </summary>
    [Attr]
    [JsonPropertyName("cityOrTown")]
    [System.ComponentModel.Description("City or town name")]
    public string? CityOrTown { get; set; }


    /// <summary>
    /// Post code
    /// </summary>
    [Attr]
    [JsonPropertyName("postalCode")]
    [System.ComponentModel.Description("Post code")]
    public string? PostalCode { get; set; }

}