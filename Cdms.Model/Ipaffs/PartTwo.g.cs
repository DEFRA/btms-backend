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
/// Part 2 of notification - Decision at inspection
/// </summary>
public partial class PartTwo  //
{


        /// <summary>
        /// Decision on the consignment
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Decision on the consignment")]
    public Decision? Decision { get; set; }

	
        /// <summary>
        /// Consignment check
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Consignment check")]
    public ConsignmentCheck? ConsignmentCheck { get; set; }

	
        /// <summary>
        /// Checks of impact of transport on animals
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Checks of impact of transport on animals")]
    public ImpactOfTransportOnAnimals? ImpactOfTransportOnAnimals { get; set; }

	
        /// <summary>
        /// Are laboratory tests required
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Are laboratory tests required")]
    public bool? LaboratoryTestsRequired { get; set; }

	
        /// <summary>
        /// Laboratory tests information details
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Laboratory tests information details")]
    public LaboratoryTests? LaboratoryTests { get; set; }

	
        /// <summary>
        /// Are the containers resealed
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Are the containers resealed")]
    public bool? ResealedContainersIncluded { get; set; }

	
        /// <summary>
        /// (Deprecated - To be removed as part of IMTA-6256) Resealed containers information details
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("(Deprecated - To be removed as part of IMTA-6256) Resealed containers information details")]
    public string[]? ResealedContainers { get; set; }

	
        /// <summary>
        /// Resealed containers information details
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Resealed containers information details")]
    public SealContainer[]? ResealedContainersMappings { get; set; }

	
        /// <summary>
        /// Control Authority information details
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Control Authority information details")]
    public ControlAuthority? ControlAuthority { get; set; }

	
        /// <summary>
        /// Controlled destination
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Controlled destination")]
    public EconomicOperator? ControlledDestination { get; set; }

	
        /// <summary>
        /// Local reference number at BIP
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Local reference number at BIP")]
    public string? BipLocalReferenceNumber { get; set; }

	
        /// <summary>
        /// Part 2 - Sometimes other user can sign decision on behalf of another user
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Part 2 - Sometimes other user can sign decision on behalf of another user")]
    public string? SignedOnBehalfOf { get; set; }

	
        /// <summary>
        /// Onward transportation
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Onward transportation")]
    public string? OnwardTransportation { get; set; }

	
        /// <summary>
        /// Validation messages for Part 2 - Decision
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Validation messages for Part 2 - Decision")]
    public ValidationMessageCode[]? ConsignmentValidations { get; set; }

	
        /// <summary>
        /// User entered date when the checks were completed
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("User entered date when the checks were completed")]
    public DateOnly? CheckedOn { get; set; }

	
        /// <summary>
        /// Accompanying documents
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Accompanying documents")]
    public AccompanyingDocument[]? AccompanyingDocuments { get; set; }

	
        /// <summary>
        /// Have the PHSI regulated commodities been auto cleared?
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Have the PHSI regulated commodities been auto cleared?")]
    public bool? PhsiAutoCleared { get; set; }

	
        /// <summary>
        /// Have the HMI regulated commodities been auto cleared?
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Have the HMI regulated commodities been auto cleared?")]
    public bool? HmiAutoCleared { get; set; }

	
        /// <summary>
        /// Inspection required
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Inspection required")]
    public string? InspectionRequired { get; set; }

	
        /// <summary>
        /// Details about the manual inspection override
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Details about the manual inspection override")]
    public InspectionOverride? InspectionOverride { get; set; }

	
        /// <summary>
        /// Date of autoclearance
        /// </summary>
    [Attr]
    [System.ComponentModel.Description("Date of autoclearance")]
    public DateTime? AutoClearedOn { get; set; }

	}


