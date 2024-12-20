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
/// Authority which performed control
/// </summary>
public partial class ControlAuthority  //
{


        /// <summary>
        /// Official veterinarian
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Official veterinarian")]
    public OfficialVeterinarian? OfficialVeterinarian { get; set; }

	
        /// <summary>
        /// Customs reference number
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Customs reference number")]
    public string? CustomsReferenceNo { get; set; }

	
        /// <summary>
        /// Were containers resealed?
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Were containers resealed?")]
    public bool? ContainerResealed { get; set; }

	
        /// <summary>
        /// When the containers are resealed they need new seal numbers
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("When the containers are resealed they need new seal numbers")]
    public string? NewSealNumber { get; set; }

	
        /// <summary>
        /// Illegal, Unreported and Unregulated (IUU) fishing reference number
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Illegal, Unreported and Unregulated (IUU) fishing reference number")]
    public string? IuuFishingReference { get; set; }

	
        /// <summary>
        /// Was Illegal, Unreported and Unregulated (IUU) check required
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Was Illegal, Unreported and Unregulated (IUU) check required")]
    public bool? IuuCheckRequired { get; set; }

	
        /// <summary>
        /// Result of Illegal, Unreported and Unregulated (IUU) check
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Result of Illegal, Unreported and Unregulated (IUU) check")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public ControlAuthorityIuuOptionEnum? IuuOption { get; set; }

	}


