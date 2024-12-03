namespace Cdms.Analytics;

public interface IMovementsAggregationService
{
    public Task<Dataset[]> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<Dataset[]> ByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<PieChartDataset> ByStatus(DateTime from, DateTime to);
}