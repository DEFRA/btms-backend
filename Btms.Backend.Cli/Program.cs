// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Btms.Backend.Cli;
using CommandLine;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

using var serviceScope = Setup.Initialise(args);
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