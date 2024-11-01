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
        app.MapPost(BaseRoute + "/notifications/", SyncNotifications).AllowAnonymous();
        app.MapPost(BaseRoute + "/clearance-requests/", SyncClearanceRequests).AllowAnonymous();
        app.MapPost(BaseRoute + "/gmrs/", SyncGmrs).AllowAnonymous();
        app.MapPost(BaseRoute + "/decisions/", SyncDecisions).AllowAnonymous();
        app.MapGet(BaseRoute + "/queueCounts/", GetQueueCounts).AllowAnonymous();
    }

    private static async Task<IResult> GetQueueCounts([FromServices] IMasterMessageBus bus)
    {
        var queueCounts = GetQueuesCount(bus);
        return Results.Ok(queueCounts);
    }

    private static async Task<IResult> SyncNotifications([FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        [FromBody] SyncNotificationsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("NOTIFICATIONS", bus);
        return Results.Ok();
    }


    private static async Task<IResult> SyncClearanceRequests([FromServices] IMediator mediator,
        [FromServices] IMasterMessageBus bus,
        [FromBody] SyncClearanceRequestsCommand command)
    {
        await mediator.Send(command);
        await WaitUntilQueueIsEmpty("ALVS", bus);
        return Results.Ok();
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
        return GetQueuesCount(bus)?.Find(x => x.QueueName == queueName).Count;
    }

    private static List<(string QueueName, int Count)> GetQueuesCount(IMasterMessageBus bus)
    {
        List<(string QueueName, int Count)> list = new List<(string QueueName, int Count)>();
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

                    list.Add((kvp.Key, queue.Count));
                }
                else
                {
                    list.Add((kvp.Key, 0));
                }
            }
        }

        return list;
    }
}