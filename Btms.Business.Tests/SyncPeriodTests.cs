using Btms.Business.Commands;
using Btms.Model;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests;

public class SyncPeriodTests
{
    [Fact]
    public void GetPeriodPaths_From202411_ReturnsCorrect()
    {
        var paths = SyncPeriod.From202411.GetPeriodPaths();

        paths.Should().Equal("/2024/11/", "/2024/12/", "/2025/01/", "/2025/02/");
    }
}