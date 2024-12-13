
using System.Net;
using Btms.Backend.Asb;
using Btms.Backend.Config;
using Btms.BlobService;
using Btms.Common.Extensions;
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
        if (options.Value.EnableManagement)
        {
            app.MapGet(BaseRoute + "/blob", GetBlobDiagnosticAsync).AllowAnonymous();
            app.MapGet(BaseRoute + "/asb", GetAzureServiceBusDiagnosticAsync).AllowAnonymous();
        }
    }
    
    private static async Task<IResult> GetBlobDiagnosticAsync(IBlobService service)
    {
        var result = await service.CheckBlobAsync(5, 1);
        if (result.Success)
        {
            return Results.Ok(result);    
        }
        return Results.Conflict(result);
    }

    private static async Task<IResult> GetAzureServiceBusDiagnosticAsync(IOptions<ServiceBusOptions> serviceBusOptions, IWebProxy proxy)
    {
        var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(serviceBusOptions.Value.Topic, serviceBusOptions.Value.Subscription)
        {
            ConnectionString = serviceBusOptions.Value.ConnectionString
        };

        var healthCheck =  new AzureServiceBusSubscriptionHealthCheck(options, new BtmsServiceBusClientProvider(proxy));
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());
        
        if (result.Status == HealthStatus.Healthy)
        {
            return Results.Ok(result);
        }
        return Results.Conflict(result);
    }
}