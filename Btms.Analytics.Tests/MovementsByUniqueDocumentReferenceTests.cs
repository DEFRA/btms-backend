using System.Text.Json.Nodes;
using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

using Btms.Analytics.Tests.Fixtures;
using Btms.Analytics.Tests.Helpers;
using TestDataGenerator.Config;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;

namespace Btms.Analytics.Tests;

public class MovementsByUniqueDocumentReferenceTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<Mrn24Gbddjer3Zfrmzar9ScenarioGenerator>(output)
{
    [Fact]
    public async Task ShouldReturnExpectedAggregation()
    {
        TestOutputHelper.WriteLine("Querying for aggregated data");
        var result = await Client
            .GetAnalyticsDashboard(["movementsByUniqueDocumentReferenceCount"],
                dateFrom: DateTime.MinValue, dateTo: DateTime.MaxValue);
           
        TestOutputHelper.WriteLine($"{result.StatusCode} status");
        result.IsSuccessStatusCode.Should().BeTrue(result.StatusCode.ToString());
        
        var chart = await result
            .AnalyticsMultiSeriesChartAs<MultiSeriesDataset<ByNumericDimensionResult>,ByNumericDimensionResult>("movementsByUniqueDocumentReferenceCount")!;

        TestOutputHelper.WriteLine("{0} aggregated items found", chart.Series!.Count);
        
        chart.ShouldBeCorrectBasedOnLinkStatusEnum();
    }
}