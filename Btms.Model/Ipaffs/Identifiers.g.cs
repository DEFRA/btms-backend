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
public partial class Identifiers  //
{


    /// <summary>
    /// Number used to identify which item the identifiers are related to
    /// </summary>
    [Attr]
    [JsonPropertyName("speciesNumber")]
    [System.ComponentModel.Description("Number used to identify which item the identifiers are related to")]
    public int? SpeciesNumber { get; set; }


    /// <summary>
    /// List of identifiers and their keys
    /// </summary>
    [Attr]
    [JsonPropertyName("data")]
    [System.ComponentModel.Description("List of identifiers and their keys")]
    public IDictionary<string, string>? Data { get; set; }


    /// <summary>
    /// Is the place of destination the permanent address?
    /// </summary>
    [Attr]
    [JsonPropertyName("isPlaceOfDestinationThePermanentAddress")]
    [System.ComponentModel.Description("Is the place of destination the permanent address?")]
    public bool? IsPlaceOfDestinationThePermanentAddress { get; set; }


    /// <summary>
    /// Permanent address of the species
    /// </summary>
    [Attr]
    [JsonPropertyName("permanentAddress")]
    [System.ComponentModel.Description("Permanent address of the species")]
    public EconomicOperator? PermanentAddress { get; set; }

}