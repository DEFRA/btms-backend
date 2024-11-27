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

public class GetImportNotificationsByArrivalDateTests
{
    private readonly IHost app;
    
    private readonly ILogger<GetImportNotificationsByArrivalDateTests> logger;
    
    private readonly IMatchingAggregationService svc;
    
    private readonly IMongoDbContext mongoContext;
    
    public GetImportNotificationsByArrivalDateTests(ITestOutputHelper testOutputHelper)
    {
        var builder = TestContextHelper.CreateBuilder<GetImportNotificationsByArrivalDateTests>(testOutputHelper);
        
        app = builder.Build();
        var rootScope = app.Services.CreateScope();
        logger = app.Services.GetRequiredService<ILogger<GetImportNotificationsByArrivalDateTests>>();
        svc = rootScope.ServiceProvider.GetRequiredService<IMatchingAggregationService>();
        mongoContext = rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();
    }
    
    [Fact]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        await mongoContext.DropCollections();
        
        var scenario = app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 3, arrivalDateRange: 10);
        await app.PushToConsumers(scenario, 1);
        
        var noMatchScenario = app.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(5, 3, arrivalDateRange: 10);
        await app.PushToConsumers(noMatchScenario, 2);
        
        var chedPScenario = app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 3, arrivalDateRange: 10);
        await app.PushToConsumers(chedPScenario, 3);
        
        logger.LogInformation("Querying for aggregated data");
        var result = (await svc
            .GetImportNotificationLinkingByArrival())
            .ToList();
        
        logger.LogInformation("{count} aggregated items found", result.Count);
        
        // logger.LogInformation(result.ToJsonString());
            
        result.Count.Should().Be(3);

        result[0].Name.Should().Be("Cveda Linked");
        result[0].Dates[0].Date.Should().BeOnOrAfter(DateOnly.FromDateTime(DateTime.Today));
        result[0].Dates.Count.Should().Be(30);
        
        result[1].Name.Should().Be("Cveda Not Linked");
        result[1].Dates[0].Date.Should().BeOnOrAfter(DateOnly.FromDateTime(DateTime.Today));
        result[1].Dates.Count.Should().Be(30);

        result[2].Name.Should().Be("Cvedp Linked");
        result[2].Dates[0].Date.Should().BeOnOrAfter(DateOnly.FromDateTime(DateTime.Today));
        result[2].Dates.Count.Should().Be(30);
    }
}