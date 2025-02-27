using Btms.Analytics.Extensions;
using Btms.Backend.Data.Healthcheck;
using Btms.BlobService;
using Btms.Business.Extensions;
using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Btms.Emf;
using Btms.Metrics;
using Btms.SyncJob.Extensions;
using Btms.Backend.Authentication;
using Btms.Backend.BackgroundTaskQueue;
using Btms.Backend.Config;
using Btms.Backend.Endpoints;
using Btms.Backend.JsonApi;
using Btms.Backend.Mediatr;
using Btms.Backend.Utils;
using Btms.Backend.Utils.Http;
using Btms.Backend.Utils.Logging;
using FluentValidation;
using HealthChecks.UI.Client;
using idunno.Authentication.Basic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Btms.Azure.Extensions;
using Environment = System.Environment;
using Btms.Backend.Asb;
using Btms.Backend.Aws;
using Btms.Business.Mediatr;
using Btms.Backend.Swagger;
using Btms.Common;
using Microsoft.FeatureManagement;

//-------- Configure the WebApplication builder------------------//

if (SwaggerGen.SwaggerGenEntrypoint(args))
    return;

var app = CreateWebApplication(args);
await app.RunAsync();

[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddFeatureManagement(builder.Configuration.GetSection("FeatureFlags"));

    ConfigureWebApplication(builder);
    ConfigureAuthentication(builder);

    var app = BuildWebApplication(builder);

    return app;
}

[ExcludeFromCodeCoverage]
static void ConfigureWebApplication(WebApplicationBuilder builder)
{
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services.AddSingleton<IBtmsMediator, BtmsMediator>();
    builder.Services.AddSyncJob();
    builder.Services.AddHostedService<QueueHostedService>();
    builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
    builder.Configuration.AddEnvironmentVariables();
    builder.Services.AddOutputCache(options =>
        {
            options.AddPolicy("Expire10Min", builder =>
                builder.Expire(TimeSpan.FromMinutes(10)));
        }
    );

    var logger = ConfigureLogging(builder);

    if (!builder.Configuration.GetValue<bool>("DisableLoadIniFile"))
    {
        builder.Configuration.AddIniFile("Properties/local.env", true)
            .AddIniFile($"Properties/local.{builder.Environment.EnvironmentName}.env", true);
    }

    builder.Services.BtmsAddOptions<ApiOptions, ApiOptions.Validator>(builder.Configuration, ApiOptions.SectionName)
        .PostConfigure(options =>
        {
            builder.Configuration.Bind(options);
            builder.Configuration.GetSection("AuthKeyStore").Bind(options);
        });

    // Load certificates into Trust Store - Note must happen before Mongo and Http client connections
    builder.Services.AddCustomTrustStore(logger);

    builder.Services.AddBusinessServices(builder.Configuration);
    builder.Services.AddConsumers(builder.Configuration);

    ConfigureEndpoints(builder);

    builder.Services.AddHttpClient();

    // calls outside the platform should be done using the named 'proxy' http client.
    builder.Services.AddHttpProxyClient();

    // The azure client has it's own way of proxying :|
    builder.Services.AddMsalHttpProxyClient(Proxy.ConfigurePrimaryHttpMessageHandler);

    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // This uses grafana for metrics and tracing and works with the local docker compose setup as well as in CDP

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddRuntimeInstrumentation()
                .AddMeter(
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Server.Kestrel",
                    "System.Net.Http",
                    MetricNames.MeterName,
                    AnalyticsMetricNames.MeterName)
                .AddPrometheusExporter();
        })
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(MetricNames.MeterName)
                .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");
        })
        .UseOtlpExporter();

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
        discovery => discovery.AddAssembly(Assembly.Load("Btms.Model")));

    builder.Services.AddJsonApiMongoDb();
    builder.Services.AddScoped<IResponseModelAdapter, BtmsResponseModelAdapter>();
    builder.Services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
    builder.Services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
    builder.Services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));

    builder.Services.AddAnalyticsServices(builder.Configuration);
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    var logBuilder = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.With<LogLevelMapper>()
        .Enrich.WithProperty("service.version", Environment.GetEnvironmentVariable("SERVICE_VERSION"))
        .WriteTo.OpenTelemetry(options =>
        {
            options.LogsEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            options.ResourceAttributes.Add("service.name", MetricNames.MeterName);
        });

    var logger = logBuilder.CreateLogger();
    builder.Logging.AddSerilog(logger);
    logger.Information("Starting application");
    return logger;
}

[ExcludeFromCodeCoverage]
static void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<IClientCredentialsManager, ClientCredentialsManager>();

    builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
        .AddBasic(options =>
        {
            options.AllowInsecureProtocol = true;
            options.Realm = "Basic Authentication";
            options.Events = new BasicAuthenticationEvents
            {
                OnValidateCredentials = async context =>
                {
                    var clientCredentialsManager = context.HttpContext.RequestServices.GetRequiredService<IClientCredentialsManager>();

                    if (await clientCredentialsManager.IsValid(context.Username, context.Password))
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                            new Claim(ClaimTypes.Name, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer)
                        };

                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                        context.Success();
                    }
                    else
                    {
                        context.Fail("Invalid Credentials");
                    }
                }
            };
        });
    builder.Services.AddAuthorization();
}

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        .AddAzureBlobStorage(sp => sp.GetService<IBlobServiceClientFactory>()!.CreateBlobServiceClient(5, 1), timeout: TimeSpan.FromSeconds(15))
        .AddMongoDb(timeout: TimeSpan.FromSeconds(15))
        .AddBtmsAzureServiceBusSubscription(TimeSpan.FromSeconds(15))
        .AddBtmsSqs(builder.Configuration);
}

[ExcludeFromCodeCoverage]
static WebApplication BuildWebApplication(WebApplicationBuilder builder)
{
    var app = builder.Build();

    // Allows us to make a global logger factory available for use where we can't get it from DI, e.g. from static functions 
    ApplicationLogging.LoggerFactory = app.Services.GetService<ILoggerFactory>();

    app.UseEmfExporter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseJsonApi();
    app.UseOutputCache();
    app.MapControllers().RequireAuthorization();

    var dotnetHealthEndpoint = "/health-dotnet";
    app.MapGet("/health", GetStatus).AllowAnonymous();
    app.MapHealthChecks(dotnetHealthEndpoint,
        new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

    var options = app.Services.GetRequiredService<IOptions<ApiOptions>>();
    app.UseSyncEndpoints(options);
    app.UseManagementEndpoints(options);
    app.UseDiagnosticEndpoints(options);
    app.UseAnalyticsEndpoints(options);

    if (builder.Environment.IsDevelopment())
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint();
    }

    return app;
}

static IResult GetStatus()
{
    return Results.Ok();
}

//Here to it can be referenced by integration tests
public partial class Program
{
    protected Program()
    {
    }
}