// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Btms.Backend.Cli;
using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;
using CommandLine;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

// Bootstrap.GeneratorClassMaps();
// var builder = Host.CreateDefaultBuilder(args)
//     .ConfigureServices((_, services) =>
//     {
//         services.AddLogging(configure => configure.AddSimpleConsole(o =>
//         {
//             o.SingleLine = true;
//             o.ColorBehavior = LoggerColorBehavior.Enabled;
//
//         }).SetMinimumLevel(LogLevel.Information));
//         services.AddTransient<App>();
//         services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
//     });
//
// var host = builder.Build();

using var serviceScope = Setup.Initialise(args); //host.Services.CreateScope();
{
    var services = serviceScope.ServiceProvider;

    try
    {
        var myService = services.GetRequiredService<App>();
        await myService.Run(args);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error Occurred {ex}");
    }
}

namespace Btms.Backend.Cli
{
    internal class App(IMediator mediator)
    {
        public Task Run(string[] args)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();

            return Parser.Default.ParseArguments(args, types)
                .WithParsedAsync(o => mediator.Send(o));
        }
    }
}