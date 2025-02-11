
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EconomicOperatorTypeEnum
{

    Consignee,

    Destination,

    Exporter,

    Importer,

    Charity,

    CommercialTransporter,

    CommercialTransporterUserAdded,

    PrivateTransporter,

    TemporaryAddress,

    PremisesOfOrigin,

    OrganisationBranchAddress,

    Packer,

    Pod,

}