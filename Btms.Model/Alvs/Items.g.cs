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


namespace Btms.Model.Alvs;

/// <summary>
/// 
/// </summary>
public partial class Items  //
{


    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public int? ItemNumber { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? CustomsProcedureCode { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? TaricCommodityCode { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? GoodsDescription { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? ConsigneeId { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? ConsigneeName { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public decimal? ItemNetMass { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public decimal? ItemSupplementaryUnits { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public decimal? ItemThirdQuantity { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public string? ItemOriginCountryCode { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public Document[]? Documents { get; set; }

	
    /// <summary>
    /// 
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("")]
    public Check[]? Checks { get; set; }

	}


