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
public partial class ServiceHeader  //
{


    /// <summary>
    /// 
    /// </summary
    [Attr]
    [System.ComponentModel.Description("")]
    public string? SourceSystem { get; set; }

	
    /// <summary>
    /// 
    /// </summary
    [Attr]
    [System.ComponentModel.Description("")]
    public string? DestinationSystem { get; set; }

	
    /// <summary>
    /// 
    /// </summary
    [Attr]
    [System.ComponentModel.Description("")]
    public string? CorrelationId { get; set; }

	
    /// <summary>
    /// 
    /// </summary
    [Attr]
    [System.ComponentModel.Description("")]
    public DateTime? ServiceCalled { get; set; }

	}

