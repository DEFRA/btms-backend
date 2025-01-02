using System.Text.Json;
using Btms.Analytics;
using Btms.Backend.Config;
using Btms.Common;
using Btms.Common.Extensions;
using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using Microsoft.AspNetCore.Mvc;

namespace Btms.Backend.Endpoints;

public static class AnalyticsEndpoints
{
	private const string BaseRoute = "analytics";
    
    public static void UseAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute + "/dashboard", GetDashboard).AllowAnonymous();
        app.MapGet(BaseRoute + "/record-current-state", RecordCurrentState).AllowAnonymous();
        app.MapGet(BaseRoute + "/timeline", Timeline);
        app.MapGet(BaseRoute + "/exceptions", Exceptions);
    }
    private static async Task<IResult> Timeline(
        [FromServices] IImportNotificationsAggregationService importService,
        [FromServices] IMovementsAggregationService movementsService,
        [FromQuery] string movementId)
    {
        var result = await movementsService.GetHistory(movementId);

        if (result.HasValue())
        {
            return TypedResults.Json(result);
        }

        return Results.NotFound();
    }
    private static async Task<IResult> Exceptions(
        [FromServices] IMovementsAggregationService movementsService,
        [FromQuery(Name = "chedType")] ImportNotificationTypeEnum[] chedTypes,
        [FromQuery(Name = "country")] string? country,
        [FromQuery(Name = "dateFrom")] DateTime? dateFrom,
        [FromQuery(Name = "dateTo")] DateTime? dateTo)
    {
        var result
            = await movementsService.GetExceptions(dateFrom ?? DateTime.MinValue, dateTo ?? DateTime.Today, chedTypes, country);

        if (result.HasValue())
        {
            return TypedResults.Json(result);
        }

        return Results.NotFound();
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
        [FromQuery] string[] chartsToRender,
        [FromQuery(Name = "chedType")] ImportNotificationTypeEnum[] chedTypes,
        [FromQuery(Name = "coo")] string? countryOfOrigin,
        [FromQuery(Name = "dateFrom")] DateTime? dateFrom,
        [FromQuery(Name = "dateTo")] DateTime? dateTo)
    {
        var logger = ApplicationLogging.CreateLogger("AnalyticsEndpoints");
        var result =
            await AnalyticsDashboards.GetCharts(logger, importService, movementsService,
                chartsToRender,
                chedTypes, countryOfOrigin, dateFrom, dateTo); 
        
        var options =
            new JsonSerializerOptions 
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = 
                {
                    new DatasetResultTypeMappingConverter<IDataset>(),
                    new DimensionResultTypeMappingConverter<IDimensionResult>() 
                }
            };
        
        return TypedResults.Json(result, options);
    }
}