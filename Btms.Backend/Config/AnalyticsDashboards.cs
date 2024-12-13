using Btms.Analytics;
using Btms.Analytics.Extensions;
using Btms.Common.Extensions;
using FluentAssertions;
using MongoDB.Driver.Linq;

namespace Btms.Backend.Config;

public static class AnalyticsDashboards
{
    public static async Task<IDictionary<string, IDataset>> GetCharts(
        ILogger logger,
        IImportNotificationsAggregationService importService,
        IMovementsAggregationService movementsService,
        string[] chartsToRender)
    {
        var charts = new Dictionary<string, Func<Task<IDataset>>>
        {
            {
                "importNotificationLinkingByCreated",
                () => importService.ByCreated(DateTime.Today.MonthAgo(), DateTime.Today).AsIDataset()
            },
            // {
            //     "importNotificationLinkingByArrival",
            //     () => importService.ByArrival(DateTime.Today.MonthAgo(), DateTime.Today.MonthLater())
            // }
        };
        //
        // var chartsToReturn = chartsToRender.Length == 0 
        //     ? charts
        //     : charts.Where(keyValuePair => chartsToRender.Contains(keyValuePair.Key));
        //
        // var tasks = chartsToReturn
        //     .Select(c => c.Value());

        // var results = await Task.WhenAll(tasks.ToArray());
            // .ToList()

        var f = charts["importNotificationLinkingByCreated"];
        var importNotificationLinkingByCreated = await f();

        logger.LogInformation("Results found {0}, {1} Series", importNotificationLinkingByCreated, ((MultiSeriesDatetimeDataset)importNotificationLinkingByCreated).Series.Count);
        return new Dictionary<string, IDataset>()
        {
            { "importNotificationLinkingByCreated", importNotificationLinkingByCreated }
        };
        // return new Dictionary<string, IDataset>()
        // {
        //     { "importNotificationLinkingByCreated", importNotificationLinkingByCreated }
        // };
        // return new { importNotificationLinkingByCreated, importNotificationLinkingByCreated };
        // return await Task.FromResult(Results.Ok(
        //     importNotificationLinkingByCreated
        // ));

        // return await Task.FromResult(Results.Ok(new {
        //     importNotificationLinkingByCreated
        // }));
        // var chartTasks = 
        // var output = Parallel.ForEachAsync(chartsToReturn, async (keyValuePair, token) => await keyValuePair.Value());
        // var output = Parallel.ForEach(chartsToReturn, (keyValuePair, token) => keyValuePair.Value());
        // return await Task.Run(() =>  //Parallel.ForEach(chartsToReturn, () =>
        // {
        //     Console.WriteLine("Testing");
        // }));

        // return await Task.Run(() => Console.WriteLine("Testing"));

        // return Task.WaitAll(chartsToReturn.Select(keyValuePair => keyValuePair.Value()));
        // foreach (var keyValuePair in chartsToReturn)
        // {
        //     if (!(chartsToRender.Length == 0) || chartsToRender.Contains(keyValuePair.Key))
        //     {
        //         var result = keyValuePair.Value();
        //     }
        // }

        // return output.ToList();

        // var importNotificationLinkingByCreated = await importService
        //     .ByCreated(DateTime.Today.MonthAgo(), DateTime.Today);
        //
        // var importNotificationLinkingByArrival = await importService
        //     .ByArrival(DateTime.Today.MonthAgo(), DateTime.Today.MonthLater());

        // var last7DaysImportNotificationsLinkingStatus = await importService
        //     .ByStatus(DateTime.Today.WeekAgo(), DateTime.Now);
        //
        // var last24HoursImportNotificationsLinkingStatus = await importService
        //     .ByStatus(DateTime.Now.Yesterday(), DateTime.Now);
        //
        // var last24HoursImportNotificationsLinkingByCreated = await importService
        //     .ByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour);
        //
        // var lastMonthImportNotificationsByTypeAndStatus = await importService
        //     .ByStatus(DateTime.Today.MonthAgo(), DateTime.Now);
        //
        // var last24HoursMovementsLinkingByCreated = await movementsService
        //     .ByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour);
        //
        // var movementsLinkingByCreated = await movementsService
        //     .ByCreated(DateTime.Today.MonthAgo(), DateTime.Today);
        //
        // var lastMonthMovementsByStatus = await movementsService
        //     .ByStatus(DateTime.Today.MonthAgo(), DateTime.Now);
        //
        // var lastMonthMovementsByItemCount = await movementsService
        //     .ByItemCount(DateTime.Today.MonthAgo(), DateTime.Now);
        //
        // var lastMonthMovementsByUniqueDocumentReferenceCount = await movementsService
        //     .ByUniqueDocumentReferenceCount(DateTime.Today.MonthAgo(), DateTime.Now);
        //
        // var lastMonthUniqueDocumentReferenceByMovementCount = await movementsService
        //     .UniqueDocumentReferenceByMovementCount(DateTime.Today.MonthAgo(), DateTime.Now);
        //
        // var lastMonthImportNotificationsByCommodityCount = await importService
        //     .ByCommodityCount(DateTime.Today.MonthAgo(), DateTime.Now);
    }
}