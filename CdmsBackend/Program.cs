using Cdms.Analytics;
using Cdms.Backend.Data.Healthcheck;
using Cdms.BlobService;
using Cdms.Business.Extensions;
using Cdms.Common.Extensions;
using Cdms.Consumers.Extensions;
using Cdms.Emf;
using Cdms.Metrics;
using Cdms.SyncJob.Extensions;
using CdmsBackend.Authentication;
using CdmsBackend.BackgroundTaskQueue;
using CdmsBackend.Config;
using CdmsBackend.Endpoints;
using CdmsBackend.JsonApi;
using CdmsBackend.Mediatr;
using CdmsBackend.Utils;
using CdmsBackend.Utils.Http;
using CdmsBackend.Utils.Logging;
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
using Environment = System.Environment;

//-------- Configure the WebApplication builder------------------//

var app = CreateWebApplication(args);
await app.RunAsync();


[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
	var builder = WebApplication.CreateBuilder(args);

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

	builder.Services.AddSingleton<ICdmsMediator, CdmsMediator>();
	builder.Services.AddSyncJob();
	builder.Services.AddHostedService<QueueHostedService>();
	builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
	builder.Configuration.AddEnvironmentVariables();

	var logger = ConfigureLogging(builder);

	if (!builder.Configuration.GetValue<bool>("DisableLoadIniFile"))
	{
		builder.Configuration.AddIniFile("Properties/local.env", true)
			.AddIniFile($"Properties/local.{builder.Environment.EnvironmentName}.env", true);
	}

	builder.Services.CdmsAddOptions<ApiOptions, ApiOptions.Validator>(builder.Configuration, ApiOptions.SectionName)
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
						MetricNames.MeterName);
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
	builder.Services.AddScoped<IResponseModelAdapter, CdmsResponseModelAdapter>();
	builder.Services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoRepository<,>));
	builder.Services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoRepository<,>));
	builder.Services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoRepository<,>));
	builder.Services.AddScoped<ILinkingAggregationService, LinkingAggregationService>();
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder builder)
{
	builder.Logging.ClearProviders();
	var logBuilder = new LoggerConfiguration()
		.ReadFrom.Configuration(builder.Configuration)
		.Enrich.With<LogLevelMapper>()
		.Enrich.WithProperty("service.version", Environment.GetEnvironmentVariable("SERVICE_VERSION"));

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
		.AddMongoDb(timeout: TimeSpan.FromSeconds(15));
}

[ExcludeFromCodeCoverage]
static WebApplication BuildWebApplication(WebApplicationBuilder builder)
{
	var app = builder.Build();


	app.UseEmfExporter();
	app.UseAuthentication();
	app.UseAuthorization();
	app.UseJsonApi();
	app.MapControllers().RequireAuthorization();

	var dotnetHealthEndpoint = "/health-dotnet";
	app.MapGet("/health", GetStatus).AllowAnonymous();
	app.MapHealthChecks(dotnetHealthEndpoint,
		new HealthCheckOptions()
		{
			Predicate = _ => true,
			ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
		});

	var options = app.Services.GetRequiredService<IOptions<ApiOptions>>();
	app.UseSyncEndpoints(options);
	app.UseManagementEndpoints(options);
	app.UseDiagnosticEndpoints(options);
	app.UseAnalyticsEndpoints();

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