using Cdms.Business.Commands;
using Cdms.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Hybrid;
using SlimMessageBus.Host.Memory;

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

    private static async Task<IResult> GetQueueCounts([FromServices] IMasterMessageBus bus)
    {
        var queueCounts = GetQueuesCount(bus);

        return queueCounts.Exists(x => x.Count > 0)
            ? Results.Json(queueCounts, statusCode: 204)
            : Results.Ok(queueCounts);
    }

    private static async Task<IResult> GetSyncNotifications(
        [FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        SyncPeriod syncPeriod)
    {
        SyncNotificationsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncNotifications(mediator, bus, command);
    }

    private static async Task<IResult> SyncNotifications([FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        [FromBody] SyncNotificationsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("NOTIFICATIONS", bus);
        return Results.Ok();
    }

    private static async Task<IResult> GetSyncClearanceRequests(
        [FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        SyncPeriod syncPeriod)
    {
        SyncClearanceRequestsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncClearanceRequests(mediator, bus, command);
    }

    private static async Task<IResult> SyncClearanceRequests(
        [FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        [FromBody] SyncClearanceRequestsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("ALVS", bus);
        return Results.Ok();
    }

    private static async Task<IResult> GetSyncGmrs(
        [FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        SyncPeriod syncPeriod)
    {
        SyncGmrsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncGmrs(mediator, bus, command);
    }

    private static async Task<IResult> SyncGmrs([FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        [FromBody] SyncGmrsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("GMR", bus);
        return Results.Ok();
    }

    private static async Task<IResult> SyncDecisions([FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        [FromBody] SyncDecisionsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("DECISIONS", bus);
        return Results.Ok();
    }

    private static async Task WaitUntilQueueIsEmpty(string queueName, IMasterMessageBus bus)
    {
        int? count = GetQueueCount(queueName, bus);
        while (count.GetValueOrDefault(0) > 0)
        {
            await Task.Delay(TimeSpan.FromMicroseconds(200));
            count = GetQueueCount(queueName, bus);
        }
    }

    private static int? GetQueueCount(string queueName, IMasterMessageBus bus)
    {
        return GetQueuesCount(bus)?.Find(x => x.Name == queueName).Count;
    }

    private static List<QueueStats> GetQueuesCount(IMasterMessageBus bus)
    {
        var list = new List<QueueStats>();
        if (bus is HybridMessageBus hybridMessageBus)
        {
            var childBus = hybridMessageBus.GetChildBus("InMemory");

            var queues =
                childBus.GetPrivateFieldValue<IDictionary<string, IMessageProcessorQueue>>(
                    "_messageProcessorQueueByPath");
            foreach (var kvp in queues)
            {
                if (kvp.Value is ConcurrentMessageProcessorQueue concurrentMessageProcessorQueue)
                {
                    var queue = concurrentMessageProcessorQueue
                        .GetPrivateFieldValue<
                            Queue<(object TransportMessage, IReadOnlyDictionary<string, object> MessageHeaders)>>(
                            "_queue");

                    list.Add(new QueueStats(kvp.Key, queue.Count));
                }
                else
                {
                    list.Add(new QueueStats(kvp.Key, 0));
                }
            }
        }

        return list;
    }

    public record QueueStats(string Name, int Count);
}