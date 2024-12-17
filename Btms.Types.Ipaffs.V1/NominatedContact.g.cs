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
/// Person to be nominated for text and email contact for the consignment
/// </summary>
public partial class NominatedContact  //
{


        /// <summary>
        /// Name of nominated contact
        /// </summary>
    [JsonPropertyName("name")]
    [Btms.SensitiveData.SensitiveData]
    public string? Name { get; set; }

	
        /// <summary>
        /// Email address of nominated contact
        /// </summary>
    [JsonPropertyName("email")]
    [Btms.SensitiveData.SensitiveData]
    public string? Email { get; set; }

	
        /// <summary>
        /// Telephone number of nominated contact
        /// </summary>
    [JsonPropertyName("telephone")]
    [Btms.SensitiveData.SensitiveData]
    public string? Telephone { get; set; }

	}


