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
    public async Task WhenCalledWithAMovementThatHasADecision_ReturnsHistory()
    {
        testOutputHelper.WriteLine("Find a suitable Movement with history");
        var movement = await multiItemDataTestFixture.MongoDbContext.Movements.Find(
            m =>
            m.Relationships.Notifications.Data.Count > 0
            && m.Decisions.Count > 0);
        
        ArgumentNullException.ThrowIfNull(movement);
        
        testOutputHelper.WriteLine("Querying for history");
        var result = await multiItemDataTestFixture
            .GetMovementsAggregationService(testOutputHelper)
            .GetHistory(movement.Id!);

        testOutputHelper.WriteLine("{0} history items found", result!.Items.Count());
        
        result.Items.Should().HasValue();
        result.Items.Select(a => a.AuditEntry.CreatedSource).Should().BeInAscendingOrder();
    }
    
    [Fact]
    public async Task WhenCalledWithAFakeMovementID_ReturnsNoHistory()
    {
        testOutputHelper.WriteLine("Querying for history");
        var result = await multiItemDataTestFixture.GetMovementsAggregationService(testOutputHelper)
            .GetHistory("");

        result.Should().BeNull();
    }
}