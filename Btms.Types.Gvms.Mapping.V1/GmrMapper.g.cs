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


namespace Btms.Types.Gvms.Mapping;

public static class GmrMapper
{
	public static Btms.Model.Gvms.Gmr Map(Btms.Types.Gvms.Gmr? from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Btms.Model.Gvms.Gmr ();
to.Id = from?.GmrId;
            to.HaulierEori = from?.HaulierEori;
            to.State = StateEnumMapper.Map(from?.State);
                to.InspectionRequired = from?.InspectionRequired;
            to.ReportToLocations = from?.ReportToLocations?.Select(x => ReportToLocationsMapper.Map(x)).ToArray();
                to.UpdatedSource = from?.UpdatedSource;
            to.Direction = DirectionEnumMapper.Map(from?.Direction);
                to.HaulierType = HaulierTypeEnumMapper.Map(from?.HaulierType);
                to.IsUnaccompanied = from?.IsUnaccompanied;
            to.VehicleRegistrationNumber = from?.VehicleRegistrationNumber;
            to.PlannedCrossing = PlannedCrossingMapper.Map(from?.PlannedCrossing);
                to.CheckedInCrossing = CheckedInCrossingMapper.Map(from?.CheckedInCrossing);
                to.ActualCrossing = ActualCrossingMapper.Map(from?.ActualCrossing);
                	return to;
	}
}

