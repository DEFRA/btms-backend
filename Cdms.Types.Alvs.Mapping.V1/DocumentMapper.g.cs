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


namespace Cdms.Types.Alvs;

public static class DocumentMapper
{
	public static Cdms.Model.Alvs.Document Map(Cdms.Types.Alvs.Document from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Cdms.Model.Alvs.Document ();
to.DocumentCode = from.DocumentCode;
            to.DocumentReference = from.DocumentReference;
            to.DocumentStatus = from.DocumentStatus;
            to.DocumentControl = from.DocumentControl;
            to.DocumentQuantity = from.DocumentQuantity;
            	return to;
	}
}

