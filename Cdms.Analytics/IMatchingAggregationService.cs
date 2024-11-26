namespace Cdms.Analytics;

public interface IMatchingAggregationService
{
    public Task<ByDateResult[]> GetImportNotificationsByMatchStatus();
}