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
/// 
/// </summary>
public partial class Identifiers  //
{


        /// <summary>
        /// Number used to identify which item the identifiers are related to
        /// </summary>
    [JsonPropertyName("speciesNumber")]
    public int? SpeciesNumber { get; set; }

	
        /// <summary>
        /// List of identifiers and their keys
        /// </summary>
    [JsonPropertyName("data")]
    public IDictionary<string, string>? Data { get; set; }

	
        /// <summary>
        /// Is the place of destination the permanent address?
        /// </summary>
    [JsonPropertyName("isPlaceOfDestinationThePermanentAddress")]
    public bool? IsPlaceOfDestinationThePermanentAddress { get; set; }

	
        /// <summary>
        /// Permanent address of the species
        /// </summary>
    [JsonPropertyName("permanentAddress")]
    public EconomicOperator? PermanentAddress { get; set; }

	}


