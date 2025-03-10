using System.Collections.Concurrent;
using System.Diagnostics;
using Btms.Analytics;
using Btms.Analytics.Extensions;
using Btms.Common.Extensions;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Driver.Linq;

namespace Btms.Backend.Config;

public class DateRange
{
    [FromQuery(Name = "dateFrom")]
    public DateTime? From { get; set; }
    [FromQuery(Name = "dateTo")]
    public DateTime? To { get; set; }
}

public static class AnalyticsDashboards
{
    public static async Task<IDictionary<string, IDataset>> GetCharts(
        ILogger logger,
        IImportNotificationsAggregationService importService,
        IMovementsAggregationService movementsService,
        string[] chartsToRender,
        ImportNotificationTypeEnum[] chedTypes,
        string? country,
        DateRange dateRange,
        // DateTime? dateFrom,
        // DateTime? dateTo,
        bool finalisedOnly
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
                () => movementsService.ByUniqueDocumentReferenceCount(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now).AsIDataset()
            },
            {
                "uniqueDocumentReferenceByMovementCount",
                () => movementsService.UniqueDocumentReferenceByMovementCount(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now).AsIDataset()
            },
            {
                "lastMonthMovementsByUniqueDocumentReferenceCount",
                () => movementsService.ByUniqueDocumentReferenceCount(DateTime.Today.MonthAgo(), DateTime.Now).AsIDataset()
            },
            {
                "movementsByUniqueDocumentReferenceCount",
                () => movementsService.ByUniqueDocumentReferenceCount(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now).AsIDataset()
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
                "decisionsByBusinessDecisionStatus",
                () => movementsService.ByBusinessDecisionStatus(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "decisionsByDecisionCode",
                () => movementsService.ByDecision(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsBySegment",
                () => movementsService.BySegment(dateRange.From ?? DateTime.Today.MonthAgo(), dateRange.To ?? DateTime.Now, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "importNotificationsByVersion",
                () => importService.ByMaxVersion(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsByMaxEntryVersion",
                () => movementsService.ByMaxVersion(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsByMaxDecisionNumber",
                () => movementsService.ByMaxAlvsDecisionNumber(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsExceptions",
                () => movementsService.ExceptionSummary(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            },
            {
                "movementsByAlvsDecisionItemNumbers",
                () => movementsService.ByAlvsDecision(dateRange.From ?? DateTime.Today.AddMonths(-3), dateRange.To ?? DateTime.Today, finalisedOnly, chedTypes, country).AsIDataset()
            }
        };

        var chartsToReturn = (chartsToRender.Length == 0
            ? charts.ToList()
            : charts.Where(keyValuePair => chartsToRender.Contains(keyValuePair.Key)).ToList());

        var results = new Dictionary<string, IDataset>();

        await Parallel.ForEachAsync(chartsToReturn, async (pair, token) =>
        {
            logger.LogInformation("Query {Chart} Starting", pair.Key);
            Stopwatch stopwatch = Stopwatch.StartNew();
            var result = await pair.Value();
            stopwatch.Stop();
            logger.LogInformation("Query {Chart} Complete in {Ms} Milliseconds", pair.Key,
                stopwatch.ElapsedMilliseconds);

            results.Add(pair.Key, result);
        });

        return results;
    }
}