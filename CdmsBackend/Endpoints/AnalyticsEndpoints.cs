using Cdms.Analytics;
using Cdms.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CdmsBackend.Endpoints;

public static class AnalyticsEndpoints
{
	private const string BaseRoute = "analytics";
    
    public static void UseAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute + "/dashboard", GetDashboard).AllowAnonymous();
    }
    
    private static async Task<IResult> GetDashboard(
        [FromServices] ILinkingAggregationService svc)
    {
        var importNotificationLinkingByCreated = await svc
            .ImportNotificationsByCreated(DateTime.Today.MonthAgo(), DateTime.Today);
        
        var importNotificationLinkingByArrival = await svc
            .ImportNotificationsByArrival(DateTime.Today.MonthAgo(), DateTime.Today.MonthLater()) ;
        
        var last7DaysImportNotificationsLinkingStatus = await svc
            .ImportNotificationsByStatus(DateTime.Today.WeekAgo(), DateTime.Now);
        
        var last24HoursImportNotificationsLinkingStatus = await svc
            .ImportNotificationsByStatus(DateTime.Now.Yesterday(), DateTime.Now);
        
        var last24HoursMovementsLinkingByCreated = await svc
            .MovementsByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour);
        
        var last24HoursImportNotificationsLinkingByCreated= await svc
            .ImportNotificationsByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour);
        
        var movementsLinkingByCreated = await svc
            .MovementsByCreated(DateTime.Today.MonthAgo(), DateTime.Today) ;
        
        var movementsLinkingByArrival = await svc
            .MovementsByArrival(DateTime.Today.MonthAgo(), DateTime.Today.MonthLater());
        
        var lastMonthImportNotificationsByTypeAndStatus = await svc
            .ImportNotificationsByStatus(DateTime.Today.MonthAgo(), DateTime.Now);
        
        var lastMonthMovementsByStatus = await svc
            .MovementsByStatus(DateTime.Today.MonthAgo(), DateTime.Now);
        
        return Results.Ok(new
        {
            importNotificationLinkingByCreated, importNotificationLinkingByArrival,
            last7DaysImportNotificationsLinkingStatus, last24HoursImportNotificationsLinkingStatus,
            last24HoursMovementsLinkingByCreated, last24HoursImportNotificationsLinkingByCreated,
            movementsLinkingByCreated, movementsLinkingByArrival,
            lastMonthImportNotificationsByTypeAndStatus, lastMonthMovementsByStatus
        });
    }
}