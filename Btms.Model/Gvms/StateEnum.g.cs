
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace Btms.Model.Gvms;

public enum StateEnum
{

    NotFinalisable,

    Open,

    Finalised,

    CheckedIn,

    Embarked,

    Completed,

}