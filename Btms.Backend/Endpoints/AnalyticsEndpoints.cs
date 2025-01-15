using System.Text.Json;
using Btms.Analytics;
using Btms.Backend.Config;
using Btms.Common;
using Btms.Common.Extensions;
using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Btms.Backend.Endpoints;

public static class AnalyticsEndpoints
{
    private const string BaseRoute = "analytics";

    public static void UseAnalyticsEndpoints(this IEndpointRouteBuilder app, IOptions<ApiOptions> options)
    {
        var dashboard = app.MapGet(BaseRoute + "/dashboard", GetDashboard)
            .AllowAnonymous();

        var timeline = app.MapGet(BaseRoute + "/timeline", Timeline);

        var exceptions = app.MapGet(BaseRoute + "/exceptions", Exceptions);

        var scenarios = app.MapGet(BaseRoute + "/scenarios", Scenarios);

        app.MapGet(BaseRoute + "/record-current-state", RecordCurrentState)
            .AllowAnonymous();

        if (!options.Value.AnalyticsCachePolicy.HasValue()) return;

        dashboard
            .CacheOutput(options.Value.AnalyticsCachePolicy);

        timeline
            .CacheOutput(options.Value.AnalyticsCachePolicy);

        exceptions
            .CacheOutput(options.Value.AnalyticsCachePolicy);

        scenarios
            .CacheOutput(options.Value.AnalyticsCachePolicy);
    }

    private static async Task<IResult> Timeline(
        [FromServices] IImportNotificationsAggregationService importService,
        [FromServices] IMovementsAggregationService movementsService,
        [FromQuery] string movementId)
    {
        var result = await movementsService
            .GetHistory(movementId);

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
            = await movementsService
                .GetExceptions(dateFrom ?? DateTime.MinValue, dateTo ?? DateTime.Today, 
                    chedTypes, country);

        return result.HasValue() ? 
            TypedResults.Json(result) : 
            Results.NotFound();
    }

    private static IResult Scenarios(
        [FromServices] IMovementsAggregationService movementsService,
        [FromServices] IImportNotificationsAggregationService importService,
        [FromQuery(Name = "dateFrom")] DateTime? dateFrom,
        [FromQuery(Name = "dateTo")] DateTime? dateTo)
    {
        var result
            = importService.Scenarios(dateFrom, dateTo);

        if (result.HasValue())
        {   
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
            await AnalyticsDashboards
                .GetCharts(logger, importService, movementsService, chartsToRender,
                    chedTypes, countryOfOrigin, dateFrom, dateTo);

        // return await SerialiseResult(result);
        
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

    // private static async Task<IResult> SerialiseResult(object result)
    // {
    //     var options =
    //         new JsonSerializerOptions
    //         {
    //             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //             Converters =
    //             {
    //                 new DatasetResultTypeMappingConverter<IDataset>(),
    //                 new DimensionResultTypeMappingConverter<IDimensionResult>()
    //             }
    //         };
    //
    //     return await Task.FromResult(TypedResults.Json(result, options));
    // }
}