using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using TestGenerator.IntegrationTesting.Backend.Fixtures;

namespace Btms.Analytics.Tests;

public class FinalisedOnlyTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(Mrn24Gbddjer3Zfrmzar9ScenarioGenerator), false, true)]
    [InlineData(typeof(Mrn24Gbddjer3Zfrmzar9ScenarioGenerator), true, false)]
    public async Task ShouldReturnCorrectAggregation(Type generatorType, bool finalisedOnly, bool returnsResults)
    {
        EnsureEnvironmentInitialised(generatorType);

        var result = await Client
            .GetAnalyticsDashboard(["decisionsByDecisionCode"],
                dateFrom: DateTime.MinValue, dateTo: DateTime.MaxValue,
                finalisedOnly: finalisedOnly);
        
        var chart = await result
            .AnalyticsChartAs<SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>>("decisionsByDecisionCode")!;
        
        chart.Summary.Values.Count
            .Should().Be(returnsResults ? 1 : 0);
    }
}