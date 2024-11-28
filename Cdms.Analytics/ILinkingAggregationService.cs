namespace Cdms.Analytics;

public enum AggregationPeriod
{
    Day,
    Hour
}

public interface ILinkingAggregationService
{
    public Task<Dataset[]> GetMovementsLinkingByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<Dataset[]> GetMovementsLinkingByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<Dataset[]> GetImportNotificationLinkingByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<Dataset[]> GetImportNotificationLinkingByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    
    public Task<PieChartDataset> GetImportNotificationLinkingStatus(DateTime from, DateTime to);
}