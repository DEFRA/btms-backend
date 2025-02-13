
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PurposePurposeGroupEnum
{

    ForImport,

    ForNONConformingConsignments,

    ForTranshipmentTo,

    ForTransitTo3rdCountry,

    ForReImport,

    ForPrivateImport,

    ForTransferTo,

    ForImportReConformityCheck,

    ForImportNonInternalMarket,

}