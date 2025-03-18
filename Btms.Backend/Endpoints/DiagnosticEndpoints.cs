using System.Net;
using Btms.Backend.Asb;
using Btms.Backend.Config;
using Btms.BlobService;
using Btms.Business;
using Btms.Common.Extensions;
using Btms.Replication;
using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Btms.Backend.Endpoints;

public static class DiagnosticEndpoints
{
    private const string BaseRoute = "diagnostics";

    public static void UseDiagnosticEndpoints(this IEndpointRouteBuilder app, IOptions<ApiOptions> options)
    {
        if (options.Value.EnableDiagnostics)
        {
            app.MapGet(BaseRoute + "/blob", GetBlobDiagnosticAsync).AllowAnonymous();
            app.MapGet(BaseRoute + "/blob-replica", GetReplicaBlobDiagnosticAsync).AllowAnonymous();
            app.MapGet(BaseRoute + "/asb", GetAzureServiceBusDiagnosticAsync).AllowAnonymous();
        }
    }

    private static async Task<IResult> GetBlobDiagnosticAsync(IBlobService service, IOptions<BusinessOptions> options)
    {
        var result = await service.CheckBlobAsync(options.Value.DmpBlobRootFolder, 5, 1);
        if (result.Success)
        {
            return Results.Ok(result);
        }
        return Results.Conflict(result);
    }

    private static async Task<IResult> GetReplicaBlobDiagnosticAsync(ReplicationTargetBlobService service)
    {
        var result = await service.CheckBlobAsync("", 5, 1);
        if (result.Success)
        {
            return Results.Ok(result);
        }
        return Results.Conflict(result);
    }

    private static async Task<IResult> GetAzureServiceBusDiagnosticAsync(HealthCheckService healthCheckService)
    {
        var healthReport = await healthCheckService.CheckHealthAsync(x => x.Name == "azuresubscription");

        if (healthReport.Status == HealthStatus.Healthy)
        {
            return Results.Ok(new { healthReport.Status });
        }
        return Results.Conflict(new { Status = healthReport.Status, Description = healthReport.Entries.FirstOrDefault().Value.Exception?.Message });

    }
}