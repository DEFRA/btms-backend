using Cdms.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace CdmsBackend.Endpoints;

public static class AnalyticsEndpoints
{
	private const string BaseRoute = "analytics";
    
    public static void UseAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute + "/import-notifications/linking-by-created", GetImportNotificationLinkingByCreated).AllowAnonymous();
        app.MapGet(BaseRoute + "/import-notifications/linking-by-arrival", GetImportNotificationLinkingByArrival).AllowAnonymous();
        app.MapGet(BaseRoute + "/get-dashboard", GetDashboard).AllowAnonymous();
    }
    private static async Task<IResult> GetImportNotificationLinkingByCreated(
        [FromServices] IMatchingAggregationService svc)
    {
        var results = await svc.GetImportNotificationLinkingByCreated();
        
        return Results.Ok(new { results = results });
    }
    
    private static async Task<IResult> GetImportNotificationLinkingByArrival(
        [FromServices] IMatchingAggregationService svc)
    {
        var results = await svc.GetImportNotificationLinkingByArrival();
        
        return Results.Ok(new { results = results });
    }
    
    private static async Task<IResult> GetDashboard(
        [FromServices] IMatchingAggregationService svc)
    {
        var importNotificationLinkingByArrival = await svc.GetImportNotificationLinkingByArrival();
        var importNotificationLinkingByCreated = await svc.GetImportNotificationLinkingByCreated();
        
        return Results.Ok(new { importNotificationLinkingByArrival, importNotificationLinkingByCreated });
    }
}