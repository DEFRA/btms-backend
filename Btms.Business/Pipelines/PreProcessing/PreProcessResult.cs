using Btms.Model.ChangeLog;
using Btms.Model.Data;

namespace Btms.Business.Pipelines.PreProcessing;

public abstract record PreProcessResult
{
    public static PreProcessingResult<T> AlreadyProcessed<T>(T record) where T : IAuditable
    {
        return new PreProcessingResult<T>(PreProcessingOutcome.AlreadyProcessed, record, null);
    }

    public static PreProcessingResult<T> Skipped<T>(T record) where T : IAuditable
    {
        return new PreProcessingResult<T>(PreProcessingOutcome.Skipped, record, null);
    }

    public static PreProcessingResult<T> Changed<T>(T record, ChangeSet changeSet) where T : IAuditable
    {
        return new PreProcessingResult<T>(PreProcessingOutcome.Changed, record, changeSet);
    }

    public static PreProcessingResult<T> New<T>(T record) where T : IAuditable
    {
        return new PreProcessingResult<T>(PreProcessingOutcome.New, record, null);
    }
}