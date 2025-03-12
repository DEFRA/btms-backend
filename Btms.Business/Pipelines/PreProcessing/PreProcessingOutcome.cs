namespace Btms.Business.Pipelines.PreProcessing;

public enum PreProcessingOutcome
{
    New,
    Changed,
    Skipped,
    AlreadyProcessed,
    NotProcessed,
    ValidationError
}