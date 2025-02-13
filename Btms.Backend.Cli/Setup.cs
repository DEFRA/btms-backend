using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Btms.Backend.Cli;

public static class Setup
{
    public static IServiceScope Initialise(string[]? args = null)
    {
        Bootstrap.GeneratorClassMaps();
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddLogging(configure => configure.AddSimpleConsole(o =>
                {
                    o.SingleLine = true;
                    o.ColorBehavior = LoggerColorBehavior.Enabled;

                }).SetMinimumLevel(LogLevel.Information));
                services.AddTransient<App>();
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
            });

        var host = builder.Build();

        return host.Services.CreateScope();
    }
}