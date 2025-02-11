
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

public enum DecisionDecisionEnum
{

    NonAcceptable,

    AcceptableForInternalMarket,

    AcceptableIfChanneled,

    AcceptableForTranshipment,

    AcceptableForTransit,

    AcceptableForTemporaryImport,

    AcceptableForSpecificWarehouse,

    AcceptableForPrivateImport,

    AcceptableForTransfer,

    HorseReEntry,

}