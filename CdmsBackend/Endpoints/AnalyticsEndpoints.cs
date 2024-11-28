using Cdms.Analytics;
using Cdms.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CdmsBackend.Endpoints;

public static class AnalyticsEndpoints
{
	private const string BaseRoute = "analytics";
    
    public static void UseAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute + "/get-dashboard", GetDashboard).AllowAnonymous();
    }
    
    private static async Task<IResult> GetDashboard(
        [FromServices] ILinkingAggregationService svc)
    {
        var importNotificationLinkingByCreated = await svc
            .GetImportNotificationLinkingByCreated(DateTime.Today.MonthAgo(), DateTime.Today);
        
        var importNotificationLinkingByArrival = await svc
            .GetImportNotificationLinkingByArrival(DateTime.Today, DateTime.Today.MonthLater()) ;
        
        var last7DaysImportNotificationsLinkingStatus = await svc
            .GetImportNotificationLinkingStatus(DateTime.Today.WeekAgo(), DateTime.Today);
        
        var last24HoursImportNotificationsLinkingStatus = await svc
            .GetImportNotificationLinkingStatus(DateTime.Now.Yesterday(), DateTime.Now);
        
        var last24HoursMovementsLinkingByCreated = await svc
            .GetMovementsLinkingByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour);
        
        var last24HoursImportNotificationsLinkingByCreated= await svc
            .GetImportNotificationLinkingByCreated(DateTime.Now.NextHour().Yesterday(), DateTime.Now.NextHour(), AggregationPeriod.Hour);
        
        var movementsLinkingByCreated = await svc
            .GetMovementsLinkingByCreated(DateTime.Today.MonthAgo(), DateTime.Today) ;
        
        var movementsLinkingByArrival = await svc
            .GetMovementsLinkingByArrival(DateTime.Today, DateTime.Today.MonthLater());
        
        return Results.Ok(new
        {
            importNotificationLinkingByCreated, importNotificationLinkingByArrival,
            last7DaysImportNotificationsLinkingStatus, last24HoursImportNotificationsLinkingStatus,
            last24HoursMovementsLinkingByCreated, last24HoursImportNotificationsLinkingByCreated,
            movementsLinkingByCreated, movementsLinkingByArrival
        });
    }
}