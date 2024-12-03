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
/// 
/// </summary>
public partial class ComplementParameterSet  //
{


        /// <summary>
        /// UUID used to match commodityComplement to its complementParameter set. CHEDPP only
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("UUID used to match commodityComplement to its complementParameter set. CHEDPP only")]
    public string? UniqueComplementId { get; set; }

	
        /// <summary>
        /// 
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public int? ComplementId { get; set; }

	
        /// <summary>
        /// 
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? SpeciesId { get; set; }

	
        /// <summary>
        /// 
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public IDictionary<string, object>? KeyDataPairs { get; set; }

	
        /// <summary>
        /// Catch certificate details
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Catch certificate details")]
    public CatchCertificates[]? CatchCertificates { get; set; }

	
        /// <summary>
        /// Data used to identify the complements inside an IMP consignment
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Data used to identify the complements inside an IMP consignment")]
    public Identifiers[]? Identifiers { get; set; }

	}


