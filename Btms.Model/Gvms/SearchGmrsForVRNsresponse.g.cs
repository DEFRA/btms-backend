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


namespace Btms.Model.Gvms;

/// <summary>
/// 
/// </summary>
public partial class SearchGmrsForVRNsresponse  //
{


        /// <summary>
        /// 
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public GmrsByVrn[]? GmrsByVrns { get; set; }

	
        /// <summary>
        /// 
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public Gmr[]? Gmrs { get; set; }

	}


