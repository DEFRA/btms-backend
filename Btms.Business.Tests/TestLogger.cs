using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Btms.Business.Tests;

internal static class TestLogger
{
    public static ILogger<T> Create<T>(ITestOutputHelper outputHelper)
    {
        var lf = new LoggerFactory();
        lf.AddXUnit(outputHelper);
        return lf.CreateLogger<T>();
    }
}