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


namespace Btms.Types.Ipaffs.Mapping;

public static class InspectionOverrideMapper
{
    public static Btms.Model.Ipaffs.InspectionOverride Map(Btms.Types.Ipaffs.InspectionOverride from)
    {
        if (from is null)
        {
            return default!;
        }

        var to = new Btms.Model.Ipaffs.InspectionOverride();
        to.OriginalDecision = from?.OriginalDecision;
        to.OverriddenOn = from?.OverriddenOn;
        to.OverriddenBy = UserInformationMapper.Map(from?.OverriddenBy!);
        return to;
    }
}

