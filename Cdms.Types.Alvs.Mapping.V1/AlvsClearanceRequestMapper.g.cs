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

public static class AlvsClearanceRequestMapper
{
	public static Cdms.Model.Alvs.AlvsClearanceRequest Map(Cdms.Types.Alvs.AlvsClearanceRequest from)
	{
	if(from is null)
	{
		return default!;
	}
		var to = new Cdms.Model.Alvs.AlvsClearanceRequest ();
to.ServiceHeader = ServiceHeaderMapper.Map(from?.ServiceHeader);
                to.Header = HeaderMapper.Map(from?.Header);
                to.Items = from?.Items?.Select(x => ItemsMapper.Map(x)).ToArray();
                	return to;
	}
}

