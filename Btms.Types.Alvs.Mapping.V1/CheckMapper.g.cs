//------------------------------------------------------------------------------
// <auto-generated>
	//     This code was generated from a template.
	//
	//     Manual changes to this file may cause unexpected behavior in your application.
	//     Manual changes to this file will be overwritten if the code is regenerated.
	//
//</auto-generated>
//------------------------------------------------------------------------------
#nullable enable


namespace Btms.Types.Alvs.Mapping;

public static class CheckMapper
{
    public static Btms.Model.Alvs.Check Map(Btms.Types.Alvs.Check from)
    {
        if (from is null)
        {
            return default!;
        }

        var to = new Btms.Model.Alvs.Check();
        to.CheckCode = from.CheckCode;
        to.DepartmentCode = from.DepartmentCode;
        to.DecisionCode = from.DecisionCode;

        to.DecisionReasons = from.DecisionReasons;
        to.DecisionsValidUntil = from.DecisionsValidUntil;
        return to;
    }
}

