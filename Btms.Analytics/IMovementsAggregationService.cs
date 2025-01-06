using Btms.Model.Auditing;
using Btms.Model.Ipaffs;

namespace Btms.Analytics;

public interface IMovementsAggregationService
{
    public Task<MultiSeriesDatetimeDataset> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<SingleSeriesDataset> ByStatus(DateTime? from = null, DateTime? to = null);
    public Task<MultiSeriesDataset> ByItemCount(DateTime from, DateTime to);
    public Task<SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>> ByDecision(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null);
    // public Task<TabularDataset<ByNameDimensionResult>> ByDecisionAndLinkStatus(DateTime from, DateTime to);
    public Task<MultiSeriesDataset> ByUniqueDocumentReferenceCount(DateTime from, DateTime to);
    public Task<SingleSeriesDataset> UniqueDocumentReferenceByMovementCount(DateTime from, DateTime to);
    // public Task<MultiSeriesDataset> ByCheck(DateTime from, DateTime to, string[]? chedTypes = null, string? country = null);
    public Task<EntityDataset<AuditHistory>?> GetHistory(string movementId);
    public Task<SingleSeriesDataset> ByMaxVersion(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null);
    public Task<SingleSeriesDataset> ByMaxDecisionNumber(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null);
    public Task<List<ExceptionResult>> GetExceptions(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null);
    public Task<SingleSeriesDataset> ExceptionSummary(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null);

}