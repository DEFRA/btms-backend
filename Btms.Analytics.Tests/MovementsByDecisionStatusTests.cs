using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using Btms.Common.Enum;
using Btms.Model.Cds;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementsByBusinessDecisionStatusTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDecisionStatusDatasetName)
{
    [Fact]
    public void ShouldHaveCorrectNumberOfMrnsInDatabase()
    {
        BackendFixture.MongoDbContext.Movements.Count()
            .Should().Be(5);
    }

    [Fact]
    public async Task ShouldFilterOutUnwantedMrns()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = await MovementsAggregationService
            .ByBusinessDecisionStatus(DateTime.MinValue, DateTime.MaxValue, false)!;

        TestOutputHelper.WriteLine("{0} aggregated items found", result!.Values);

        result.Values.Sum(r => r.Value).Should().Be(4);
    }

    [Fact]
    public async Task ShouldNotHaveCancelledOrDestroyed()
    {
        var result = await MovementsAggregationService
            .ByBusinessDecisionStatus(DateTime.MinValue, DateTime.MaxValue, false)!;

        result.Values.Keys.Should().NotContain(BusinessDecisionStatusEnum.CancelledOrDestroyed.GetValue());
    }

    [Fact]
    public async Task ShouldHaveManualReleases()
    {
        var result = await MovementsAggregationService
            .ByBusinessDecisionStatus(DateTime.MinValue, DateTime.MaxValue, false)!;

        result.Values.Keys.Should().Contain(BusinessDecisionStatusEnum.ManualReleases.GetValue());
    }

    [Fact]
    public async Task ShouldHaveMatchComplete()
    {
        var result = await MovementsAggregationService
            .ByBusinessDecisionStatus(DateTime.MinValue, DateTime.MaxValue, false)!;

        result.Values.Keys.Should().Contain(BusinessDecisionStatusEnum.MatchComplete.GetValue());
    }

    [Fact]
    public async Task ShouldHaveAlvsReleaseBtmsNotReleased()
    {
        var result = await MovementsAggregationService
            .ByBusinessDecisionStatus(DateTime.MinValue, DateTime.MaxValue, false)!;

        result.Values.Keys.Should().Contain(BusinessDecisionStatusEnum.AlvsReleaseBtmsNotReleased.GetValue());
    }

    // [Fact]
    // public async Task ShouldHaveAnythingElse()
    // {
    //     var result = await MovementsAggregationService
    //         .ByBusinessDecisionStatus(DateTime.MinValue, DateTime.MaxValue, false)!;
    //
    //     result.Values.Keys.Should().Contain(BusinessDecisionStatusEnum.AnythingElse.GetValue());
    // }
}