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


namespace Cdms.Types.Gmr;

/// <summary>
/// 
/// </summary>
public partial class SearchGmrsForDeclarationIdsResponse  //
{


        /// <summary>
        /// 
        /// </summary>
    [JsonPropertyName("gmrByDeclarationId")]
    public GmrByDeclarationId[]? GmrByDeclarationIds { get; set; }

	
        /// <summary>
        /// 
        /// </summary>
    [JsonPropertyName("gmrs")]
    public Gmr[]? Gmrs { get; set; }

	}


