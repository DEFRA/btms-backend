using Btms.Model.ChangeLog;
using Btms.Model.Data;

namespace Btms.Business.Pipelines.PreProcessing;

public record PreProcessingResult<T>(
    PreProcessingOutcome Outcome,
    T Record,
    ChangeSet? ChangeSet) : PreProcessResult
    where T : IAuditable
{

    public bool IsCreatedOrChanged()
    {
        return Record.GetLatestAuditEntry().IsCreatedOrUpdated();
    }

    public bool IsDeleted()
    {
        return Record.GetLatestAuditEntry().IsDeleted();
    }

}