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

public static class DecisionMapper
{
    public static Btms.Model.Cds.CdsClearanceRequest Map(Btms.Types.Alvs.Decision from)
    {
        if (from is null)
        {
            return default!;
        }

        var to = new Btms.Model.Cds.CdsClearanceRequest();
        to.ServiceHeader = ServiceHeaderMapper.Map(from?.ServiceHeader!);
        to.Header = HeaderMapper.Map(from?.Header!);
        to.Items = from?.Items?.Select(x => ItemsMapper.Map(x)).ToArray();
        return to;
    }
}

