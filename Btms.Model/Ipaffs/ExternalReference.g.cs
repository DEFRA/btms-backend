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
/// Reference number from an external system which is related to this notification
/// </summary>
public partial class ExternalReference  //
{


        /// <summary>
        /// Identifier of the external system to which the reference relates
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Identifier of the external system to which the reference relates")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public ExternalReferenceSystemEnum? System { get; set; }

	
        /// <summary>
        /// Reference which is added to the notification when either sent to the downstream system or received from it
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Reference which is added to the notification when either sent to the downstream system or received from it")]
    public string? Reference { get; set; }

	
        /// <summary>
        /// Details whether there&#x27;s an exact match between the external source and IPAFFS data
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Details whether there's an exact match between the external source and IPAFFS data")]
    public bool? ExactMatch { get; set; }

	
        /// <summary>
        /// Details whether an importer has verified the data from an external source
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Details whether an importer has verified the data from an external source")]
    public bool? VerifiedByImporter { get; set; }

	
        /// <summary>
        /// Details whether an inspector has verified the data from an external source
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Details whether an inspector has verified the data from an external source")]
    public bool? VerifiedByInspector { get; set; }

	}


