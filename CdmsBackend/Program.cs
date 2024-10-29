using CdmsBackend.Endpoints;
using CdmsBackend.Example.Endpoints;
using CdmsBackend.Example.Services;
using CdmsBackend.Utils;
using CdmsBackend.Utils.Http;
using CdmsBackend.Utils.Logging;
using CdmsBackend.Utils.Mongo;
using FluentValidation;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;
using Cdms.Backend.Data.Extensions;
using Cdms.Business.Extensions;
using Microsoft.Extensions.Options;
using Cdms.BlobService;
using Cdms.SensitiveData;
using FluentAssertions.Common;
using System.Text.Json.Serialization;

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

    var logger = ConfigureLogging(builder);

    // Load certificates into Trust Store - Note must happen before Mongo and Http client connections
    builder.Services.AddCustomTrustStore(logger);

    builder.Services.AddBusinessServices(builder.Configuration);

    ConfigureMongoDb(builder);

    ConfigureEndpoints(builder);

    builder.Services.AddHttpClient();

    // calls outside the platform should be done using the named 'proxy' http client.
    builder.Services.AddHttpProxyClient(logger);

    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    var logger = new LoggerConfiguration()
        .WriteTo.Seq("http://localhost:6341")
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.With<LogLevelMapper>()
        .CreateLogger();
    builder.Logging.AddSerilog(logger);
    logger.Information("Starting application");
    return logger;
}

[ExcludeFromCodeCoverage]
static void ConfigureMongoDb(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<IMongoDbClientFactory>(_ =>
        new MongoDbClientFactory(builder.Configuration.GetValue<string>("Mongo:DatabaseUri")!,
            builder.Configuration.GetValue<string>("Mongo:DatabaseName")!));
}

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplicationBuilder builder)
{
    // our Example service, remove before deploying!
    builder.Services.AddSingleton<IExamplePersistence, ExamplePersistence>();

    builder.Services.AddControllers().AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    builder.Services.AddHealthChecks();
}

[ExcludeFromCodeCoverage]
static WebApplication BuildWebApplication(WebApplicationBuilder builder)
{
    var app = builder.Build();

    app.UseRouting();
    app.MapHealthChecks("/health");

    // Example module, remove before deploying!
    app.UseExampleEndpoints();
    app.UseSyncEndpoints();

    return app;
}