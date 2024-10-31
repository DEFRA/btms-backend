// using FluentValidation.Results;
// using MongoDB.Bson;

using System.Reflection;
using Cdms.Business.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Hybrid;
using SlimMessageBus.Host.Memory;

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
        app.MapGet(BaseRoute + "/queueCounts/", GetQueueCounts).AllowAnonymous();
    }

    private static async Task<IResult> GetQueueCounts([FromServices] IMasterMessageBus bus)
    {
        if (bus is HybridMessageBus hybridMessageBus)
        {
            var childBus = hybridMessageBus.GetChildBus("InMemory");

            var field = childBus.GetType()
                .GetField("_messageProcessorQueueByPath", BindingFlags.Instance | BindingFlags.NonPublic);

            var queues = (IDictionary<string, IMessageProcessorQueue>)field.GetValue(childBus);
            return Results.Ok(queues.Select(x =>
            {
                if (x.Value is ConcurrentMessageProcessorQueue concurrentMessageProcessorQueue)
                {
                    var queueField = concurrentMessageProcessorQueue.GetType()
                        .GetField("_queue", BindingFlags.Instance | BindingFlags.NonPublic);

                    var queue =
                        (Queue<(object TransportMessage, IReadOnlyDictionary<string, object> MessageHeaders)>)
                        queueField.GetValue(concurrentMessageProcessorQueue);
                    return new { Name = x.Key, Count = queue.Count };
                }

                return new { Name = x.Key, Count = 0 };
            }));
        }

        return Results.Ok();
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
        if (bus is HybridMessageBus hybridMessageBus)
        {
            var childBus = hybridMessageBus.GetChildBus("InMemory");

            var field = childBus.GetType()
                .GetField("_messageProcessorQueueByPath", BindingFlags.Instance | BindingFlags.NonPublic);

            var queues = (IDictionary<string, IMessageProcessorQueue>)field.GetValue(childBus);
            var queueCounts = queues.Select(x =>
            {
                if (x.Value is ConcurrentMessageProcessorQueue concurrentMessageProcessorQueue)
                {
                    var queueField = concurrentMessageProcessorQueue.GetType()
                        .GetField("_queue", BindingFlags.Instance | BindingFlags.NonPublic);

                    var queue =
                        (Queue<(object TransportMessage, IReadOnlyDictionary<string, object> MessageHeaders)>)
                        queueField.GetValue(concurrentMessageProcessorQueue);
                    return new { Name = x.Key, Count = queue.Count };
                }

                return new { Name = x.Key, Count = 0 };
            });

            return queueCounts?.FirstOrDefault(x => x.Name == queueName)?.Count;
        }

        return 0;
    }
}