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

public static class HeaderMapper
{
	public static Btms.Model.Alvs.Header Map(Btms.Types.Alvs.Header from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Btms.Model.Alvs.Header ();
to.EntryReference = from.EntryReference;
            to.EntryVersionNumber = from.EntryVersionNumber;
            to.PreviousVersionNumber = from.PreviousVersionNumber;
            to.DeclarationUcr = from.DeclarationUcr;
            to.DeclarationPartNumber = from.DeclarationPartNumber;
            to.DeclarationType = from.DeclarationType;
            to.ArrivesAt = from.ArrivalDateTime;
            to.SubmitterTurn = from.SubmitterTurn;
            to.DeclarantId = from.DeclarantId;
            to.DeclarantName = from.DeclarantName;
            to.DispatchCountryCode = from.DispatchCountryCode;
            to.GoodsLocationCode = from.GoodsLocationCode;
            to.MasterUcr = from.MasterUcr;
            	return to;
	}
}

