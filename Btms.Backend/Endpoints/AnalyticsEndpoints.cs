using System.Text.Json;
using Btms.Analytics;
using Btms.Backend.Config;
using Btms.Common;
using Btms.Common.Extensions;
using Btms.Model.Extensions;
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
        [FromServices] IMovementsAggregationService movementsService,
        [FromQuery] string[] chartsToRender)
    {
        var logger = ApplicationLogging.CreateLogger("AnalyticsEndpoints");
        var result = await AnalyticsDashboards.GetCharts(logger, importService, movementsService, chartsToRender); 
        
        var options =
            new JsonSerializerOptions 
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = 
                {
                    new ResultTypeMappingConverter<IDataset>(),
                    // new ResultTypeMappingConverter<IDataset, MultiSeriesDataset>(),
                    // new ResultTypeMappingConverter<IDataset, SingleSeriesDataset>() 
                }
            };
        
        return TypedResults.Json(result, options);
    }
}