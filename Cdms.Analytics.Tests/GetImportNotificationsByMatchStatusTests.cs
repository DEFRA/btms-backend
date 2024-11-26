using Cdms.Analytics.Tests.Helpers;
using Cdms.Backend.Data;
using Cdms.Backend.Data.Extensions;
using Cdms.Business.Extensions;
using Cdms.Consumers.Extensions;
using Cdms.Model.Extensions;
using Cdms.SyncJob.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;
using TestDataGenerator.Scenarios;
using Xunit;
using Xunit.Abstractions;

namespace Cdms.Analytics.Tests;

public class GetImportNotificationsByMatchStatusTests
{
    private readonly IHost app;
    
    private readonly ILogger<GetImportNotificationsByMatchStatusTests> logger;
    
    private readonly IMatchingAggregationService svc;
    
    private readonly IMongoDbContext mongoContext;
    
    public GetImportNotificationsByMatchStatusTests(ITestOutputHelper testOutputHelper)
    {
        var builder = TestContextHelper.CreateBuilder(testOutputHelper);
        
        app = builder.Build();
        logger = app.Services.GetRequiredService<ILogger<GetImportNotificationsByMatchStatusTests>>();
        svc = app.Services.GetRequiredService<IMatchingAggregationService>();
        mongoContext = app.Services.GetRequiredService<IMongoDbContext>();
    }
    
    [Fact]
    public async Task WhenCalledWithSmallMatchAndNoMatchSet_ReturnExpectedAggregation()
    {
        await mongoContext.DropCollections();
        
        var scenario = app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 2);
        await app.PushToConsumers(scenario, 1);
        
        var noMatchScenario = app.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(5, 2);
        await app.PushToConsumers(noMatchScenario, 2);

        var chedPScenario = app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 1);
        await app.PushToConsumers(chedPScenario, 3);
        
        var result = (await svc
            .GetImportNotificationsByMatchStatus())
            .OrderBy(r => r.Date)
            .ThenBy(r => r.BucketVariables["Matched"])
            .ThenBy(r => r.BucketVariables["ChedType"])
            .ToList();
        
        logger.LogInformation(result.ToJsonString());
            
        result.Count.Should().Be(5);
        
        result[0].Date.Should().Be(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));
        result[0].Value.Should().Be(5);
        result[0].BucketVariables["Matched"].Should().Be("False");
        result[0].BucketVariables["ChedType"].Should().Be("Cveda");
        
        result[1].Date.Should().Be(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));
        result[1].Value.Should().Be(10);
        result[1].BucketVariables["Matched"].Should().Be("True");
        result[1].BucketVariables["ChedType"].Should().Be("Cveda");
        
        result[2].Date.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        result[2].Value.Should().Be(5);
        result[2].BucketVariables["Matched"].Should().Be("False");
        result[2].BucketVariables["ChedType"].Should().Be("Cveda");
        
        result[3].Date.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        result[3].Value.Should().Be(10);
        result[3].BucketVariables["Matched"].Should().Be("True");
        result[3].BucketVariables["ChedType"].Should().Be("Cveda");
        
        result[4].Date.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        result[4].Value.Should().Be(1);
        result[4].BucketVariables["Matched"].Should().Be("True");
        result[4].BucketVariables["ChedType"].Should().Be("Cvedp");
    }
}