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


namespace Btms.Types.Gvms;

/// <summary>
/// 
/// </summary>
public partial class SearchGmrsRequest  //
{


        /// <summary>
        /// A list of TRNs to search for GMRs.
        /// </summary>
    [JsonPropertyName("trailerRegistrationNums")]
    public string[]? TrailerRegistrationNums { get; set; }

	}


