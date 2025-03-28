
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Ipaffs;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImportNotificationStatusEnum
{

    Draft,

    Submitted,

    Validated,

    Rejected,

    InProgress,

    Amend,

    Modify,

    Replaced,

    Cancelled,

    Deleted,

    PartiallyRejected,

    SplitConsignment,

}