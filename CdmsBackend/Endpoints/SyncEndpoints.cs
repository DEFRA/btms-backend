// using FluentValidation.Results;
// using MongoDB.Bson;

using Cdms.Business.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CdmsBackend.Endpoints;

public static class SyncEndpoints
{
    private const string BaseRoute = "sync";
    // private static ILogger Logger = ApplicationLogging.CreateLogger("SyncEndpoints");
    // private static ILogger Logger = ApplicationLogging.CreateLogger("SyncEndpoints");

    //private static SyncPeriod Parse(string? period)
    //{
    //    if (string.IsNullOrEmpty(period))
    //    {
    //        return SyncPeriod.All;
    //    }

    //    return Enum.Parse<SyncPeriod>(period, true);

    //    // if (Enum.TryParse<SyncPeriod>(period, true, out SyncPeriod typed))
    //    // {
    //    //     return typed;
    //    // }
    //    // return SyncPeriod.All;
    //}
    public static void UseSyncEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(BaseRoute + "/notifications/", SyncNotifications).AllowAnonymous();
        app.MapPost(BaseRoute + "/clearance-requests/", SyncClearanceRequests).AllowAnonymous();
        app.MapPost(BaseRoute + "/gmrs/", SyncGmrs).AllowAnonymous();
        app.MapPost(BaseRoute + "/decisions/", SyncDecisions).AllowAnonymous();
    }


    private static async Task<IResult> SyncNotifications([FromServices] IMediator mediator,
        [FromBody] SyncNotificationsCommand command)
    {
        await mediator.Send(command);
        return Results.Ok();
    }

    private static async Task<IResult> SyncClearanceRequests([FromServices] IMediator mediator,
        [FromBody] SyncClearanceRequestsCommand command)
    {
        await mediator.Send(command);
        return Results.Ok();
    }

    private static async Task<IResult> SyncGmrs([FromServices] IMediator mediator,
        [FromBody] SyncGmrsCommand command)
    {
        await mediator.Send(command);
        return Results.Ok();
    }

    private static async Task<IResult> SyncDecisions([FromServices] IMediator mediator,
        [FromBody] SyncDecisionsCommand command)
    {
        await mediator.Send(command);
        return Results.Ok();
    }
}