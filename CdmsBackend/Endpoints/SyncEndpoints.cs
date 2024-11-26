using Cdms.Business;
using Cdms.Business.Commands;
using Cdms.Consumers.MemoryQueue;
using Cdms.SyncJob;
using CdmsBackend.Config;
using CdmsBackend.Mediatr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CdmsBackend.Endpoints;

public static class SyncEndpoints
{
    private const string BaseRoute = "sync";

    public static void UseSyncEndpoints(this IEndpointRouteBuilder app, IOptions<ApiOptions> options)
    {
        if (options.Value.EnableSync)
        {
            app.MapGet(BaseRoute + "/import-notifications/", GetSyncNotifications).AllowAnonymous();
            app.MapPost(BaseRoute + "/import-notifications/", SyncNotifications).AllowAnonymous();
            app.MapGet(BaseRoute + "/clearance-requests/", GetSyncClearanceRequests).AllowAnonymous();
            app.MapPost(BaseRoute + "/clearance-requests/", SyncClearanceRequests).AllowAnonymous();
        }

        app.MapGet(BaseRoute + "/gmrs/", GetSyncGmrs).AllowAnonymous();
        app.MapPost(BaseRoute + "/gmrs/", SyncGmrs).AllowAnonymous();
        app.MapPost(BaseRoute + "/decisions/", SyncDecisions).AllowAnonymous();
        app.MapGet(BaseRoute + "/queue-counts/", GetQueueCounts).AllowAnonymous();
        app.MapGet(BaseRoute + "/jobs/", GetAllSyncJobs).AllowAnonymous();
        app.MapGet(BaseRoute + "/jobs/clear", ClearSyncJobs).AllowAnonymous();
		app.MapGet(BaseRoute + "/jobs/{jobId}", GetSyncJob).AllowAnonymous();
    }

    private static Task<IResult> GetAllSyncJobs([FromServices] ISyncJobStore store)
    {
        return Task.FromResult(Results.Ok(new { jobs = store.GetJobs() }));
    }

    private static Task<IResult> ClearSyncJobs([FromServices] ISyncJobStore store)
    {
		store.ClearSyncJobs();
	    return Task.FromResult(Results.Ok());
    }

	private static Task<IResult> GetSyncJob([FromServices] ISyncJobStore store, string jobId)
    {
        return Task.FromResult(Results.Ok(store.GetJobs().Find(x => x.JobId == Guid.Parse(jobId))));
    }

    private static Task<IResult> GetQueueCounts([FromServices] IMemoryQueueStatsMonitor queueStatsMonitor)
    {
       return Task.FromResult(queueStatsMonitor.GetAll().Any(x => x.Value.Count > 0)
            ? Results.Ok(queueStatsMonitor.GetAll())
            : Results.NoContent());
    }

    private static async Task<IResult> GetSyncNotifications(
        [FromServices] ICdmsMediator mediator,
        SyncPeriod syncPeriod)
    {
        SyncNotificationsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncNotifications(mediator, command);
    }

    private static async Task<IResult> SyncNotifications([FromServices] ICdmsMediator mediator,
        [FromBody] SyncNotificationsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);
       
    }

    private static async Task<IResult> GetSyncClearanceRequests(
        [FromServices] ICdmsMediator mediator,
        SyncPeriod syncPeriod)
    {
        SyncClearanceRequestsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncClearanceRequests(mediator, command);
    }

    private static async Task<IResult> SyncClearanceRequests(
        [FromServices] ICdmsMediator mediator,
        [FromBody] SyncClearanceRequestsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);
    }

    private static async Task<IResult> GetSyncGmrs(
        [FromServices] ICdmsMediator mediator,
        SyncPeriod syncPeriod)
    {
        SyncGmrsCommand command = new() { SyncPeriod = syncPeriod };
        return await SyncGmrs(mediator, command);
    }

    private static async Task<IResult> SyncGmrs([FromServices] ICdmsMediator mediator,
        [FromBody] SyncGmrsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);
    }

    private static async Task<IResult> SyncDecisions([FromServices] ICdmsMediator mediator,
        [FromBody] SyncDecisionsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);
    }
}