
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Types.Ipaffs;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum EconomicOperatorTypeEnum
{

    [EnumMember(Value = "consignee")]
    Consignee,

    [EnumMember(Value = "destination")]
    Destination,

    [EnumMember(Value = "exporter")]
    Exporter,

    [EnumMember(Value = "importer")]
    Importer,

    [EnumMember(Value = "charity")]
    Charity,

    [EnumMember(Value = "commercial transporter")]
    CommercialTransporter,

    [EnumMember(Value = "commercial transporter - user added")]
    CommercialTransporterUserAdded,

    [EnumMember(Value = "private transporter")]
    PrivateTransporter,

    [EnumMember(Value = "temporary address")]
    TemporaryAddress,

    [EnumMember(Value = "premises of origin")]
    PremisesOfOrigin,

    [EnumMember(Value = "organisation branch address")]
    OrganisationBranchAddress,

    [EnumMember(Value = "packer")]
    Packer,

    [EnumMember(Value = "pod")]
    Pod,

}