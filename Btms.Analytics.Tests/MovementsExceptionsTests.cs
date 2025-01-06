using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestGenerator.IntegrationTesting.Backend;

namespace Btms.Analytics.Tests;

public class MovementsExceptionsTests(ITestOutputHelper output)
    : ScenarioDatasetBaseTest(output, Datasets.FunctionalAnalyticsDatasetName)
{
    
    [Fact(Skip="Change to an individiual scenario test that generates an exception. Possibly a seperate test for each exception type")]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = (await GetMovementsAggregationService()
            .ByDecision(DateTime.Today.MonthAgo(), DateTime.Today.Tomorrow()))
            .Result;

        TestOutputHelper.WriteLine("{0} aggregated items found", result.Count);
        
        result.Count.Should().BeGreaterThan(1);
    }
}