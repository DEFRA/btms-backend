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

public static class SingleLaboratoryTestMapper
{
    public static Btms.Model.Ipaffs.SingleLaboratoryTest Map(Btms.Types.Ipaffs.SingleLaboratoryTest from)
    {
        if (from is null)
        {
            return default!;
        }

        var to = new Btms.Model.Ipaffs.SingleLaboratoryTest();
        to.CommodityCode = from?.CommodityCode;
        to.SpeciesId = from?.SpeciesId;
        to.TracesId = from?.TracesId;
        to.TestName = from?.TestName;
        to.Applicant = ApplicantMapper.Map(from?.Applicant!);
        to.LaboratoryTestResult = LaboratoryTestResultMapper.Map(from?.LaboratoryTestResult!);
        return to;
    }
}

