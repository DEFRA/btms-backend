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


namespace Btms.Model.Cds;

/// <summary>
/// 
/// </summary>
public partial class Document  //
{


    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? DocumentCode { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? DocumentReference { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? DocumentStatus { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? DocumentControl { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public decimal? DocumentQuantity { get; set; }

	}

