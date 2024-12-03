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
/// Catch certificate details for uploaded attachment
/// </summary>
public partial class CatchCertificateDetails  //
{


        /// <summary>
        /// The UUID of the catch certificate
        /// </summary>
    [JsonPropertyName("catchCertificateId")]
    public string? CatchCertificateId { get; set; }

	
        /// <summary>
        /// Catch certificate reference
        /// </summary>
    [JsonPropertyName("catchCertificateReference")]
    public string? CatchCertificateReference { get; set; }

	
        /// <summary>
        /// Catch certificate date of issue
        /// </summary>
    [JsonPropertyName("dateOfIssue")]
    public DateTime? DateOfIssue { get; set; }

	
        /// <summary>
        /// Catch certificate flag state of catching vessel(s)
        /// </summary>
    [JsonPropertyName("flagState")]
    public string? FlagState { get; set; }

	
        /// <summary>
        /// List of species imported under this catch certificate
        /// </summary>
    [JsonPropertyName("species")]
    public string[]? Species { get; set; }

	}


