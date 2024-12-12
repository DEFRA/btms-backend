/*------------------------------------------------------------------------------
<auto-generated>
    This code was generated from the InternalClass template.
    Manual changes to this file may cause unexpected behavior in your application.
    Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
using JsonApiDotNetCore.Resources.Annotations;
using System.Text.Json.Serialization;
using System.Dynamic;

namespace Btms.Model.Ipaffs;

/// <summary>
/// Purpose of consignment details
/// </summary>
public partial class Purpose //
{

    /// <summary>
    /// Does consignment conforms to UK laws
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Does consignment conforms to UK laws")]
    public bool? ConformsToEU { get; set; }

    /// <summary>
    /// Detailed purpose of internal market purpose group
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Detailed purpose of internal market purpose group")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public PurposeInternalMarketPurposeEnum? InternalMarketPurpose { get; set; }

    /// <summary>
    /// Country that consignment is transshipped through
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Country that consignment is transshipped through")]
    public string? ThirdCountryTranshipment { get; set; }

    /// <summary>
    /// Detailed purpose for non conforming purpose group
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Detailed purpose for non conforming purpose group")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public PurposeForNonConformingEnum? ForNonConforming { get; set; }

    /// <summary>
    /// There are 3 types of registration number based on the purpose of consignment. Customs registration number, Free zone registration number and Shipping supplier registration number.
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("There are 3 types of registration number based on the purpose of consignment. Customs registration number, Free zone registration number and Shipping supplier registration number.")]
    public string? RegNumber { get; set; }

    /// <summary>
    /// Ship name
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Ship name")]
    public string? ShipName { get; set; }

    /// <summary>
    /// Destination Ship port
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Destination Ship port")]
    public string? ShipPort { get; set; }

    /// <summary>
    /// Exit Border Inspection Post
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Exit Border Inspection Post")]
    public string? ExitBip { get; set; }

    /// <summary>
    /// Country to which consignment is transited
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Country to which consignment is transited")]
    public string? ThirdCountry { get; set; }

    /// <summary>
    /// Countries that consignment is transited through
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Countries that consignment is transited through")]
    public string[]? TransitThirdCountries { get; set; }

    /// <summary>
    /// Specification of Import or admission purpose
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Specification of Import or admission purpose")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public PurposeForImportOrAdmissionEnum? ForImportOrAdmission { get; set; }

    /// <summary>
    /// Exit date when import or admission
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Exit date when import or admission")]
    public DateOnly? ExitDate { get; set; }

    /// <summary>
    /// Final Border Inspection Post
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Final Border Inspection Post")]
    public string? FinalBip { get; set; }

    /// <summary>
    /// Purpose group of consignment (general purpose)
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("Purpose group of consignment (general purpose)")]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public PurposePurposeGroupEnum? PurposeGroup { get; set; }

    /// <summary>
    /// DateTime
    /// </summary>
    [Attr]
    [System.ComponentModel.Description("DateTime")]
    public DateTime? EstimatedArrivesAtPortOfExit { get; set; }
}
