using Btms.Common.Extensions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using FluentAssertions;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementHistoryTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    [Fact]
    public async Task WhenCalledWithAMovementThatHasADecision_ReturnsHistory()
    {
        TestOutputHelper.WriteLine("Find a suitable Movement with history");
        var movement = await MongoDbContext.Movements.Find(
            m =>
            m.Relationships.Notifications.Data.Count > 0
            && m.Decisions.Count > 0);
        
        ArgumentNullException.ThrowIfNull(movement);
        
        TestOutputHelper.WriteLine("Querying for history");
        var result = await GetMovementsAggregationService()
            .GetHistory(movement.Id!);

        TestOutputHelper.WriteLine("{0} history items found", result!.Items.Count());
        
        result.Items.Should().HasValue();
        result.Items.Select(a => a.AuditEntry.CreatedSource).Should().BeInAscendingOrder();
    }
    
    [Fact]
    public async Task WhenCalledWithAFakeMovementID_ReturnsNoHistory()
    {
        TestOutputHelper.WriteLine("Querying for history");
        var result = await GetMovementsAggregationService()
            .GetHistory("");

        result.Should().BeNull();
    }
}