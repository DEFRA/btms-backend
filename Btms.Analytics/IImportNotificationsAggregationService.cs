using Btms.Model.Ipaffs;

namespace Btms.Analytics;

public interface IImportNotificationsAggregationService
{
    public Task<MultiSeriesDatetimeDataset> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<MultiSeriesDatetimeDataset> ByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<SingleSeriesDataset> ByStatus(DateTime from, DateTime to);
    public Task<MultiSeriesDataset> ByCommodityCount(DateTime from, DateTime to);
    public Task<SingleSeriesDataset> ByMaxVersion(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null);
}