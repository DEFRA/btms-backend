using Btms.Common.Extensions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using FluentAssertions;

namespace Btms.Analytics.Tests;

[Collection(nameof(MultiItemDataTestCollection))]
public class MovementHistoryTests(
    MultiItemDataTestFixture multiItemDataTestFixture,
    ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task WhenCalled_ReturnsHistory()
    {
        testOutputHelper.WriteLine("Querying for history");
        var result = await multiItemDataTestFixture
            .GetMovementsAggregationService(testOutputHelper)
            .GetHistory("23GB9999001215000001");

        testOutputHelper.WriteLine("{0} history items found", result!.Items.Count());
        
        result.Items.Should().HasValue();
        result.Items.Select(a => a.AuditEntry.CreatedSource).Should().BeInAscendingOrder();
    }
    
    [Fact]
    public async Task WhenCalled_ReturnsNoHistory()
    {
        testOutputHelper.WriteLine("Querying for history");
        var result = await multiItemDataTestFixture.GetMovementsAggregationService(testOutputHelper)
            .GetHistory("");

        result.Should().BeNull();
    }
}