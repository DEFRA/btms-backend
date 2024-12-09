using Btms.Analytics;
using Btms.Analytics.Extensions;
using Btms.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Btms.Backend.Endpoints;

public static class AnalyticsEndpoints
{
	private const string BaseRoute = "analytics";
    
    public static void UseAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute + "/dashboard", GetDashboard).AllowAnonymous();
        app.MapGet(BaseRoute + "/record-current-state", RecordCurrentState).AllowAnonymous();
    }
    private static async Task<IResult> RecordCurrentState(
        [FromServices] ImportNotificationMetrics importNotificationMetrics)
    {
        await importNotificationMetrics.RecordCurrentState();
        return Results.Ok();
    }

    private static async Task<IResult> GetDashboard(
        [FromServices] IImportNotificationsAggregationService importService,
        [FromServices] IMovementsAggregationService movementsService)
    {
        var importNotificationLinkingByCreated = await importService
            .ByCreated(DateTime.Today.MonthAgo(), DateTime.Today);

        var importNotificationLinkingByArrival = await importService
            .ByArrival(DateTime.Today.MonthAgo(), DateTime.Today.MonthLater());

        var last7DaysImportNotificationsLinkingStatus = await importService
            .ByStatus(DateTime.Today.WeekAgo(), DateTime.Now);

        var last24HoursImportNotificationsLinkingStatus = await importService
            .ByStatus(DateTime.Now.Yesterday(), DateTime.Now);

        var last24HoursImportNotificationsLinkingByCreated = await importService
            .ByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour);

        var lastMonthImportNotificationsByTypeAndStatus = await importService
            .ByStatus(DateTime.Today.MonthAgo(), DateTime.Now);

        var last24HoursMovementsLinkingByCreated = await movementsService
            .ByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour);

        var movementsLinkingByCreated = await movementsService
            .ByCreated(DateTime.Today.MonthAgo(), DateTime.Today);

        var lastMonthMovementsByStatus = await movementsService
            .ByStatus(DateTime.Today.MonthAgo(), DateTime.Now);

        var lastMonthMovementsByItemCount = await movementsService
            .ByItemCount(DateTime.Today.MonthAgo(), DateTime.Now);

        var lastMonthMovementsByUniqueDocumentReferenceCount = await movementsService
            .ByUniqueDocumentReferenceCount(DateTime.Today.MonthAgo(), DateTime.Now);

        var lastMonthUniqueDocumentReferenceByMovementCount = await movementsService
            .UniqueDocumentReferenceByMovementCount(DateTime.Today.MonthAgo(), DateTime.Now);

        var lastMonthImportNotificationsByCommodityCount = await importService
            .ByCommodityCount(DateTime.Today.MonthAgo(), DateTime.Now);

        return Results.Ok(new
        {
            importNotificationLinkingByCreated,
            importNotificationLinkingByArrival,
            last7DaysImportNotificationsLinkingStatus,
            last24HoursImportNotificationsLinkingStatus,
            last24HoursMovementsLinkingByCreated,
            last24HoursImportNotificationsLinkingByCreated,
            movementsLinkingByCreated,
            lastMonthMovementsByStatus,
            lastMonthMovementsByItemCount,
            lastMonthImportNotificationsByCommodityCount,
            lastMonthMovementsByUniqueDocumentReferenceCount,
            lastMonthUniqueDocumentReferenceByMovementCount
        });
    }
}