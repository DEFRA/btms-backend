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

public static class LaboratoryTestResultMapper
{
    public static Btms.Model.Ipaffs.LaboratoryTestResult Map(Btms.Types.Ipaffs.LaboratoryTestResult from)
    {
        if (from is null)
        {
            return default!;
        }

        var to = new Btms.Model.Ipaffs.LaboratoryTestResult();
        to.SampleUseByDate = from?.SampleUseByDate;
        to.ReleasedOn = from?.ReleasedDate;
        to.LaboratoryTestMethod = from?.LaboratoryTestMethod;
        to.Results = from?.Results;
        to.Conclusion = LaboratoryTestResultConclusionEnumMapper.Map(from?.Conclusion);
        to.LabTestCreatedOn = from?.LabTestCreatedDate;
        return to;
    }
}

