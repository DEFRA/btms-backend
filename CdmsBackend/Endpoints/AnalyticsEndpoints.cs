using Cdms.Analytics;
using Cdms.Business.Commands;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CdmsBackend.Endpoints;

public static class AnalyticsEndpoints
{
    private const string BaseRoute = "analytics";

    public static void UseAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute + "/matching", GetMatchingAnalyticsAsync).AllowAnonymous();
    }
    private static async Task<IResult> GetMatchingAnalyticsAsync(
        [FromServices] IMatchingAggregationService svc)
    {
        var results = await svc.GetImportNotificationsByMatchStatus();
        
        return Results.Ok(new { results = results });
    }

}