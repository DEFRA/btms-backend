using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestGenerator.IntegrationTesting.Backend.Extensions;

public static class TestOutputHelperExtensions
{
    public static ILoggerFactory GetLoggerFactory(this ITestOutputHelper helper)
    {
        var loggerProvider = new XUnitLoggerProvider(helper, new XUnitLoggerOptions());
        var factory = new LoggerFactory([loggerProvider]);

        return factory;
    }

    public static ILogger<T> GetLogger<T>(this ITestOutputHelper helper)
    {
        var factory = helper.GetLoggerFactory();

        return factory.CreateLogger<T>();
    }
}