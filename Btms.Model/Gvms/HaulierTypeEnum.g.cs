
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Gvms;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HaulierTypeEnum
{

    Standard,

    FpoAsn,

    FpoOther,

    NatoMod,

    Rmg,

    Etoe,

}