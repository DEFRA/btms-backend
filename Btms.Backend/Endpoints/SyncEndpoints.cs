using Btms.Backend.Config;
using Btms.Backend.Mediatr;
using Btms.Business.Commands;
using Btms.Business.Mediatr;
using Btms.Consumers.MemoryQueue;
using Btms.SyncJob;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlimMessageBus.Host;

namespace Btms.Backend.Endpoints;

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

            app.MapGet(BaseRoute + "/gmrs/", GetSyncGmrs).AllowAnonymous();
            app.MapPost(BaseRoute + "/gmrs/", SyncGmrs).AllowAnonymous();

            app.MapGet(BaseRoute + "/decisions/", GetSyncDecisions).AllowAnonymous();
            app.MapPost(BaseRoute + "/decisions/", SyncDecisions).AllowAnonymous();

            app.MapGet(BaseRoute + "/finalisations/", GetSyncFinalisations).AllowAnonymous();
            app.MapPost(BaseRoute + "/finalisations/", SyncFinalisations).AllowAnonymous();

        }

        app.MapPost(BaseRoute + "/generate-download", GenerateDownload).AllowAnonymous();
        app.MapGet(BaseRoute + "/download/{id}", DownloadNotifications).AllowAnonymous();


        app.MapGet(BaseRoute + "/queue-counts/", GetQueueCounts).AllowAnonymous();
        app.MapGet(BaseRoute + "/jobs/", GetAllSyncJobs).AllowAnonymous();
        app.MapGet(BaseRoute + "/jobs/clear", ClearSyncJobs).AllowAnonymous();
        app.MapGet(BaseRoute + "/jobs/{jobId}", GetSyncJob).AllowAnonymous();
        app.MapGet(BaseRoute + "/jobs/{jobId}/cancel", CancelSyncJob).AllowAnonymous();
    }

    internal static async Task<IResult> InitialiseEnvironment(IHost app, SyncPeriod period, string? rootFolder, InitialisationStrategy? strategy, bool dropCollections)
    {
        var mediator = app.Services.GetRequiredService<IBtmsMediator>();

        InitialiseCommand command = new() { SyncPeriod = period, RootFolder = rootFolder, Strategy = strategy, DropCollections = dropCollections };

        await mediator.SendSyncJob(command);

        return Results.Ok();
    }

    private static IResult DownloadNotifications([FromServices] IWebHostEnvironment env, string id)
    {
        var stream = File.OpenRead($"{env.ContentRootPath}/{id}.zip");
        return Results.File(stream, "application/zip", $"{id}.zip", enableRangeProcessing: true);
    }

    private static async Task<IResult> GenerateDownload([FromServices] IBtmsMediator mediator, [FromBody] DownloadCommand command)
    {
        await mediator.SendJob(command);
        return Results.Ok(command.JobId);
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

    private static Task<IResult> CancelSyncJob([FromServices] ISyncJobStore store, string jobId)
    {
        var job = store.GetJobs().Find(x => x.JobId == Guid.Parse(jobId));

        if (job is null)
        {
            return Task.FromResult(Results.NoContent());
        }
        job.Cancel();
        return Task.FromResult(Results.Ok());
    }

    private static Task<IResult> GetQueueCounts([FromServices] IMemoryQueueStatsMonitor queueStatsMonitor)
    {
        return Task.FromResult(queueStatsMonitor.GetAll().Any(x => x.Value.Count > 0)
             ? Results.Ok(queueStatsMonitor.GetAll())
             : Results.NoContent());
    }

    private static async Task<IResult> GetSyncNotifications(
        [FromServices] IBtmsMediator mediator,
        SyncPeriod syncPeriod,
        string? rootFolder)
    {
        SyncNotificationsCommand command = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
        return await SyncNotifications(mediator, command);
    }

    private static async Task<IResult> SyncNotifications([FromServices] IBtmsMediator mediator,
        [FromBody] SyncNotificationsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);

    }

    private static async Task<IResult> GetSyncClearanceRequests(
        [FromServices] IBtmsMediator mediator,
        SyncPeriod syncPeriod,
        string? rootFolder)
    {
        SyncClearanceRequestsCommand command = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
        return await SyncClearanceRequests(mediator, command);
    }

    private static async Task<IResult> SyncClearanceRequests(
        [FromServices] IBtmsMediator mediator,
        [FromBody] SyncClearanceRequestsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);
    }

    private static async Task<IResult> GetSyncGmrs(
        [FromServices] IBtmsMediator mediator,
        SyncPeriod syncPeriod,
        string? rootFolder)
    {
        SyncGmrsCommand command = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
        return await SyncGmrs(mediator, command);
    }

    private static async Task<IResult> SyncGmrs([FromServices] IBtmsMediator mediator,
        [FromBody] SyncGmrsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);
    }

    private static async Task<IResult> GetSyncDecisions(
        [FromServices] IBtmsMediator mediator,
        SyncPeriod syncPeriod,
        string? rootFolder)
    {
        SyncDecisionsCommand command = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
        return await SyncDecisions(mediator, command);
    }

    private static async Task<IResult> SyncDecisions([FromServices] IBtmsMediator mediator,
        [FromBody] SyncDecisionsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);
    }

    private static async Task<IResult> GetSyncFinalisations(
        [FromServices] IBtmsMediator mediator,
        SyncPeriod syncPeriod,
        string? rootFolder)
    {
        SyncFinalisationsCommand command = new() { SyncPeriod = syncPeriod, RootFolder = rootFolder };
        return await SyncFinalisations(mediator, command);
    }

    private static async Task<IResult> SyncFinalisations([FromServices] IBtmsMediator mediator,
        [FromBody] SyncFinalisationsCommand command)
    {
        await mediator.SendSyncJob(command);
        return Results.Accepted($"/sync/jobs/{command.JobId}", command.JobId);
    }
}