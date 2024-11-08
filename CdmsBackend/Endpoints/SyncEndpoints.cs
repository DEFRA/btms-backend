using Cdms.Business;
using Cdms.Business.Commands;
using Cdms.Consumers.MemoryQueue;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CdmsBackend.Endpoints;

public static class SyncEndpoints
{
    private const string BaseRoute = "sync";

    public static void UseSyncEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute + "/import-notifications/", GetSyncNotifications).AllowAnonymous();
        app.MapPost(BaseRoute + "/import-notifications/", SyncNotifications).AllowAnonymous();
        app.MapGet(BaseRoute + "/clearance-requests/", GetSyncClearanceRequests).AllowAnonymous();
        app.MapPost(BaseRoute + "/clearance-requests/", SyncClearanceRequests).AllowAnonymous();
        app.MapGet(BaseRoute + "/gmrs/", GetSyncGmrs).AllowAnonymous();
        app.MapPost(BaseRoute + "/gmrs/", SyncGmrs).AllowAnonymous();
        app.MapPost(BaseRoute + "/decisions/", SyncDecisions).AllowAnonymous();
        app.MapGet(BaseRoute + "/queue-counts/", GetQueueCounts).AllowAnonymous();
    }

    private static async Task<IResult> GetQueueCounts([FromServices] IMemoryQueueStatsMonitor queueStatsMonitor)
    {
       return queueStatsMonitor.GetAll().Any(x => x.Value.Count > 0)
            ? Results.Ok(queueStatsMonitor.GetAll())
            : Results.NoContent();
    }

    private static async Task<IResult> GetSyncNotifications(
        [FromServices] IMediator mediator,
        [FromServices] IMemoryQueueStatsMonitor queueStatsMonitor,
        SyncPeriod syncPeriod)
    {
        SyncNotificationsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncNotifications(mediator, queueStatsMonitor, command);
    }

    private static async Task<IResult> SyncNotifications([FromServices] IMediator mediator,
        [FromServices] IMemoryQueueStatsMonitor queueStatsMonitor,
        [FromBody] SyncNotificationsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("NOTIFICATIONS", queueStatsMonitor);
        return Results.Ok();
    }

    private static async Task<IResult> GetSyncClearanceRequests(
        [FromServices] IMediator mediator,
        [FromServices] IMemoryQueueStatsMonitor queueStatsMonitor,
        SyncPeriod syncPeriod)
    {
        SyncClearanceRequestsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncClearanceRequests(mediator, queueStatsMonitor, command);
    }

    private static async Task<IResult> SyncClearanceRequests(
        [FromServices] IMediator mediator,
        [FromServices] IMemoryQueueStatsMonitor queueStatsMonitor,
        [FromBody] SyncClearanceRequestsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("ALVS", queueStatsMonitor);
        return Results.Ok();
    }

    private static async Task<IResult> GetSyncGmrs(
        [FromServices] IMediator mediator,
        [FromServices] IMemoryQueueStatsMonitor queueStatsMonitor,
        SyncPeriod syncPeriod)
    {
        SyncGmrsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncGmrs(mediator, queueStatsMonitor, command);
    }

    private static async Task<IResult> SyncGmrs([FromServices] IMediator mediator,
        [FromServices] IMemoryQueueStatsMonitor queueStatsMonitor,
        [FromBody] SyncGmrsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("GMR", queueStatsMonitor);
        return Results.Ok();
    }

    private static async Task<IResult> SyncDecisions([FromServices] IMediator mediator,
        [FromServices] IMemoryQueueStatsMonitor queueStatsMonitor,
        [FromBody] SyncDecisionsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("DECISIONS", queueStatsMonitor);
        return Results.Ok();
    }

    private static async Task WaitUntilQueueIsEmpty(string queueName, IMemoryQueueStatsMonitor queueMonitor)
    {
        int? count = GetQueueCount(queueName, queueMonitor);
        while (count.GetValueOrDefault(0) > 0)
        {
            await Task.Delay(TimeSpan.FromMicroseconds(200));
            count = GetQueueCount(queueName, queueMonitor);
        }
    }

    private static int? GetQueueCount(string queueName, IMemoryQueueStatsMonitor queueMonitor)
    {
        return queueMonitor.GetAll()[queueName].Count;
    }
}