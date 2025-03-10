using Btms.Model.Ipaffs;

namespace Btms.Analytics;

public interface IImportNotificationsAggregationService
{
    public Task<MultiSeriesDatetimeDataset> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<MultiSeriesDatetimeDataset> ByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<SingleSeriesDataset> ByStatus(DateTime? from = null, DateTime? to = null);
    public Task<MultiSeriesDataset<ByNumericDimensionResult>> ByCommodityCount(DateTime from, DateTime to);
    public Task<SingleSeriesDataset> ByMaxVersion(DateTime from, DateTime to, bool finalisedOnly, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null);
    public EntityDataset<ScenarioItem> Scenarios(DateTime? from, DateTime? to);
}