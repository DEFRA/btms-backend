namespace Cdms.Analytics;

public interface IMatchingAggregationService
{
    public Task<Dataset[]> GetImportNotificationLinkingByCreated();
    public Task<Dataset[]> GetImportNotificationLinkingByArrival();
}