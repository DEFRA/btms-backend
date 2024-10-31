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


namespace Cdms.Model.Ipaffs;

/// <summary>
/// Party details
/// </summary>
public partial class Party  //
{


        /// <summary>
        /// IPAFFS ID of party
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("IPAFFS ID of party")]
    public string? Id { get; set; }

	
        /// <summary>
        /// Name of party
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Name of party")]
    public string? Name { get; set; }

	
        /// <summary>
        /// Company ID
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Company ID")]
    public string? CompanyId { get; set; }

	
        /// <summary>
        /// Contact ID (B2C)
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Contact ID (B2C)")]
    public string? ContactId { get; set; }

	
        /// <summary>
        /// Company name
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Company name")]
    public string? CompanyName { get; set; }

	
        /// <summary>
        /// Addresses
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Addresses")]
    public string[]? Addresses { get; set; }

	
        /// <summary>
        /// County
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("County")]
    public string? County { get; set; }

	
        /// <summary>
        /// Post code of party
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Post code of party")]
    public string? PostCode { get; set; }

	
        /// <summary>
        /// Country of party
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Country of party")]
    public string? Country { get; set; }

	
        /// <summary>
        /// City
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("City")]
    public string? City { get; set; }

	
        /// <summary>
        /// TRACES ID
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("TRACES ID")]
    public int? TracesId { get; set; }

	
        /// <summary>
        /// Type of party
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Type of party")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public PartyTypeEnum? Type { get; set; }

	
        /// <summary>
        /// Approval number
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Approval number")]
    public string? ApprovalNumber { get; set; }

	
        /// <summary>
        /// Phone number of party
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Phone number of party")]
    public string? Phone { get; set; }

	
        /// <summary>
        /// Fax number of party
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Fax number of party")]
    public string? Fax { get; set; }

	
        /// <summary>
        /// Email number of party
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Email number of party")]
    public string? Email { get; set; }

	}


