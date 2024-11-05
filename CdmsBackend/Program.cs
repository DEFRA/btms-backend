using CdmsBackend.Endpoints;
using CdmsBackend.Utils;
using CdmsBackend.Utils.Http;
using CdmsBackend.Utils.Logging;
using FluentValidation;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;
using Cdms.Business.Extensions;
using Cdms.BlobService;
using Cdms.Backend.Data.Healthcheck;
using Cdms.Business;
using Cdms.Consumers.Extensions;
using CdmsBackend.Config;
using HealthChecks.UI.Client;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;

//-------- Configure the WebApplication builder------------------//

var app = CreateWebApplication(args);
await app.RunAsync();


[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureWebApplication(builder);

    var app = BuildWebApplication(builder);

    return app;
}

[ExcludeFromCodeCoverage]
static void ConfigureWebApplication(WebApplicationBuilder builder)
{
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddIniFile("Properties/local.env", true)
        .AddIniFile($"Properties/local.{builder.Environment.EnvironmentName}.env", true);

    builder.Services.AddOptions<ApiOptions>()
        .Bind(builder.Configuration.GetSection(ApiOptions.SectionName))
        .ValidateDataAnnotations();
    
    builder.Services.AddOptions<BusinessOptions>()
        .Bind(builder.Configuration.GetSection(BusinessOptions.SectionName))
        .ValidateDataAnnotations();
    
    var logger = ConfigureLogging(builder);

    // Load certificates into Trust Store - Note must happen before Mongo and Http client connections
    builder.Services.AddCustomTrustStore(logger);

    builder.Services.AddBusinessServices(builder.Configuration);
    builder.Services.AddConsumers(builder.Configuration);

    ConfigureEndpoints(builder);

    builder.Services.AddHttpClient();

    // calls outside the platform should be done using the named 'proxy' http client.
    builder.Services.AddHttpProxyClient(logger);

    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // This added Open Telemetry, and export to look at metrics locally.
    // The Aspire dashboard can be used to view these metrics  :
    // docker run --rm -it -p 18888:18888 -p 4317:18889 -d --name aspire-dashboard -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS='true' mcr.microsoft.com/dotnet/nightly/aspire-dashboard:8.0.0-preview.6

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "System.Net.Http",
                        "Cdms");
            });

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }
    else
    {
        // Assumes we're in CDP... - this doesn't exist
        // builder.Services.AddOpenTelemetry().UseEmfExporter()
    }

    static void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        options.Namespace = "api";
        options.UseRelativeLinks = true;
        options.IncludeTotalResourceCount = true;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.ClientIdGeneration = ClientIdGenerationMode.Allowed;
#if DEBUG
        options.IncludeExceptionStackTraceInErrors = true;
        options.IncludeRequestBodyInErrors = true;
        options.SerializerOptions.WriteIndented = true;
#endif
    }

    builder.Services.AddJsonApi(ConfigureJsonApiOptions,
        discovery => discovery.AddAssembly(Assembly.Load("Cdms.Model")));

    builder.Services.AddJsonApiMongoDb();
    builder.Services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
    builder.Services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
    builder.Services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    var logBuilder = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.With<LogLevelMapper>();

    if (builder.Environment.IsDevelopment())
    {
        logBuilder
            .WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                options.ResourceAttributes.Add("service.name", "cdmsbackend");
            });
    }

    var logger = logBuilder.CreateLogger();
    builder.Logging.AddSerilog(logger);
    logger.Information("Starting application");
    return logger;
}

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        .AddAzureBlobStorage(sp => sp.GetService<IBlobServiceClientFactory>()!.CreateBlobServiceClient())
        .AddMongoDb();
}

[ExcludeFromCodeCoverage]
static WebApplication BuildWebApplication(WebApplicationBuilder builder)
{
    var app = builder.Build();


    app.UseJsonApi();
    app.MapControllers();

    app.MapHealthChecks("/health",
        new HealthCheckOptions()
        {
            Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

    app.UseSyncEndpoints();
    app.UseManagementEndpoints(app.Services.GetRequiredService<IOptions<ApiOptions>>());

    return app;
}

//Here to it can be referenced by integration tests
public partial class Program
{
    protected Program()
    {
    }
}