namespace Btms.Analytics;

public interface IMovementsAggregationService
{
    public Task<MultiSeriesDatetimeDataset> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day);
    public Task<SingleSeriesDataset> ByStatus(DateTime from, DateTime to);
    public Task<MultiSeriesDataset> ByItemCount(DateTime from, DateTime to);
    public Task<MultiSeriesDataset> ByUniqueDocumentReferenceCount(DateTime from, DateTime to);
    public Task<SingleSeriesDataset> UniqueDocumentReferenceByMovementCount(DateTime from, DateTime to);
    public Task<MultiSeriesDataset> ByCheck(DateTime from, DateTime to);
}