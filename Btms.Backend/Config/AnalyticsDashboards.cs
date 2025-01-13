using Btms.Analytics;
using Btms.Analytics.Extensions;
using Btms.Common.Extensions;
using Btms.Model.Ipaffs;
using FluentAssertions;
using MongoDB.Driver.Linq;

namespace Btms.Backend.Config;

public static class AnalyticsDashboards
{
    public static async Task<IDictionary<string, IDataset>> GetCharts(
        ILogger logger,
        IImportNotificationsAggregationService importService,
        IMovementsAggregationService movementsService,
        string[] chartsToRender,
        ImportNotificationTypeEnum[] chedTypes,
        string? country,
        DateTime? dateFrom,
        DateTime? dateTo
        )
    {
        var charts = new Dictionary<string, Func<Task<IDataset>>>
        {
            {
                "importNotificationLinkingByCreated",
                () => importService.ByCreated(DateTime.Today.MonthAgo(), DateTime.Today).AsIDataset()
            },
            {
                "importNotificationLinkingByArrival",
                () => importService.ByArrival(DateTime.Today.MonthAgo(), DateTime.Today.MonthLater()).AsIDataset()
            },
            {
                "last7DaysImportNotificationsLinkingStatus",
                () => importService.ByStatus(DateTime.Today.WeekAgo(), DateTime.Now).AsIDataset()
            },
            {
                "last24HoursImportNotificationsLinkingStatus",
                () => importService.ByStatus(DateTime.Now.Yesterday(), DateTime.Now).AsIDataset()
            },
            {
                "last24HoursImportNotificationsLinkingByCreated",
                () => importService
                    .ByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour)
                    .AsIDataset()
            },
            {
                "lastMonthImportNotificationsByTypeAndStatus",
                () => importService.ByStatus(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "allImportNotificationsByTypeAndStatus",
                () => importService.ByStatus().AsIDataset()
            },
            {
                "last24HoursMovementsLinkingByCreated",
                () => movementsService.ByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour).AsIDataset()
            },
            {
                "movementsLinkingByCreated",
                () => movementsService.ByCreated(DateTime.Today.MonthAgo(), DateTime.Today).AsIDataset()
            },
            {
                "allMovementsByStatus",
                () => movementsService.ByStatus().AsIDataset()
            },
            {
                "lastMonthMovementsByStatus",
                () => movementsService.ByStatus(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "lastMonthMovementsByItemCount",
                () => movementsService.ByItemCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "uniqueDocumentReferenceCount",
                () => movementsService.ByUniqueDocumentReferenceCount(dateFrom ?? DateTime.Today.MonthAgo(), dateTo ?? DateTime.Now).AsIDataset()
            },
            {
                "uniqueDocumentReferenceByMovementCount",
                () => movementsService.UniqueDocumentReferenceByMovementCount(dateFrom ?? DateTime.Today.MonthAgo(), dateTo ?? DateTime.Now).AsIDataset()
            },
            {
                "lastMonthMovementsByUniqueDocumentReferenceCount",
                () => movementsService.ByUniqueDocumentReferenceCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "movementsByUniqueDocumentReferenceCount",
                () => movementsService.ByUniqueDocumentReferenceCount(dateFrom ?? DateTime.Today.MonthAgo(), dateTo ?? DateTime.Now).AsIDataset()
            },
            {
                "lastMonthUniqueDocumentReferenceByMovementCount",
                () => movementsService.UniqueDocumentReferenceByMovementCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "lastMonthImportNotificationsByCommodityCount",
                () => importService.ByCommodityCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "decisionsByDecisionCode",
                () => movementsService.ByDecision(dateFrom ?? DateTime.Today.MonthAgo(), dateTo ?? DateTime.Now, chedTypes, country).AsIDataset()
            },
            {
                "importNotificationsByVersion",
                () => importService.ByMaxVersion(dateFrom ?? DateTime.Today.AddMonths(-3), dateTo ?? DateTime.Today, chedTypes, country).AsIDataset()
            },
            {
                "movementsByMaxEntryVersion",
                () => movementsService.ByMaxVersion(dateFrom ?? DateTime.Today.AddMonths(-3), dateTo ?? DateTime.Today, chedTypes, country).AsIDataset()
            },
            {
                "movementsByMaxDecisionNumber",
                () => movementsService.ByMaxDecisionNumber(dateFrom ?? DateTime.Today.AddMonths(-3), dateTo ?? DateTime.Today, chedTypes, country).AsIDataset()
            },
            {
                "movementsExceptions",
                () => movementsService.ExceptionSummary(dateFrom ?? DateTime.Today.AddMonths(-3), dateTo ?? DateTime.Today, chedTypes, country).AsIDataset()
            }
        };
        
        var chartsToReturn = chartsToRender.Length == 0
            ? charts
            : charts.Where(keyValuePair => chartsToRender.Contains(keyValuePair.Key));
            
        var taskList = chartsToReturn.Select(r => new KeyValuePair<string, Task<IDataset>>(key:r.Key, value: r.Value()));
        
        // TODO - have just noticed this executes each chart twice
        // once during Task.WhenAll and again on the following line - revisit
        await Task.WhenAll(taskList.Select(r => r.Value));

        var output = taskList
            .ToDictionary(t => t.Key, t => t.Value.Result);
        
        logger.LogInformation("Results found {0} Datasets, {1}", output.Count, output.Keys);

        return output;
        
    }
}