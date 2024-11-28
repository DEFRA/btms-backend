namespace Cdms.Analytics;

public enum AggregationPeriod
{
    Day,
    Hour
}

public interface ILinkingAggregationService
{
    public Task<Dataset[]> MovementsByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<Dataset[]> MovementsByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<PieChartDataset> MovementsByStatus(DateTime from, DateTime to);
    public Task<Dataset[]> ImportNotificationsByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<Dataset[]> ImportNotificationsByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    
    public Task<PieChartDataset> ImportNotificationsByStatus(DateTime from, DateTime to);
}