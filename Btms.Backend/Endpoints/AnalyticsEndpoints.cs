using System.Text.Json;
using Btms.Analytics;
using Btms.Backend.Config;
using Btms.Common;
using Btms.Common.Extensions;
using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        [AsParameters] DateRange dateRange,
        [FromQuery(Name = "chedType")] ImportNotificationTypeEnum[] chedTypes,
        [FromQuery(Name = "country")] string? country,
        [FromQuery(Name = "finalisedOnly")] bool finalisedOnly = true)
    {
        var result
            = await movementsService
                .GetExceptions(dateRange.From ?? DateTime.MinValue, dateRange.To ?? DateTime.Today,
                    finalisedOnly, chedTypes, country);

        return result.HasValue() ?
            TypedResults.Json(result) :
            Results.NotFound();
    }

    private static IResult Scenarios(
        [FromServices] IMovementsAggregationService movementsService,
        [FromServices] IImportNotificationsAggregationService importService,
        [AsParameters] DateRange dateRange)
    {
        var result
            = importService.Scenarios(dateRange.From, dateRange.To);

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
        [AsParameters] DateRange dateRange,
        [FromQuery] string[] chartsToRender,
        [FromQuery(Name = "chedType")] ImportNotificationTypeEnum[] chedTypes,
        [FromQuery(Name = "coo")] string? countryOfOrigin,
        [FromQuery(Name = "finalisedOnly")] bool finalisedOnly = true)
    {
        var logger = ApplicationLogging.CreateLogger("AnalyticsEndpoints");


        var result =
            await AnalyticsDashboards
                .GetCharts(logger, importService, movementsService, chartsToRender,
                    chedTypes, countryOfOrigin, dateRange, finalisedOnly);

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