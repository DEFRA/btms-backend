
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccompanyingDocumentDocumentTypeEnum
{

    AirWaybill,

    BillOfLading,

    CargoManifest,

    CatchCertificate,

    CommercialDocument,

    CommercialInvoice,

    ConformityCertificate,

    ContainerManifest,

    CustomsDeclaration,

    Docom,

    HealthCertificate,

    HeatTreatmentCertificate,

    ImportPermit,

    InspectionCertificate,

    Itahc,

    JourneyLog,

    LaboratorySamplingResultsForAflatoxin,

    LatestVeterinaryHealthCertificate,

    LetterOfAuthority,

    LicenseOrAuthorisation,

    MycotoxinCertification,

    OriginCertificate,

    Other,

    PhytosanitaryCertificate,

    ProcessingStatement,

    ProofOfStorage,

    RailwayBill,

    SeaWaybill,

    VeterinaryHealthCertificate,

    ListOfIngredients,

    PackingList,

    RoadConsignmentNote,

}