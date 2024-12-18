using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Btms.Analytics.Tests.Helpers;

public static class ITestOutputHelperExtensions
{
    public static ILogger<T> GetLogger<T>(this ITestOutputHelper helper)
    {
        var loggerProvider = new XUnitLoggerProvider(helper, new XUnitLoggerOptions());
        var factory = new LoggerFactory([loggerProvider]);
        
        return factory.CreateLogger<T>();
    }
}