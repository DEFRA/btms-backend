using Cdms.Analytics.Tests.Helpers;
using Cdms.Backend.Data;
using Cdms.Model.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Scenarios;
using Xunit;
using Xunit.Abstractions;

namespace Cdms.Analytics.Tests;

public class GetImportNotificationsByCreatedDateTests
{
    private readonly IHost app;

    private readonly ILogger<GetImportNotificationsByCreatedDateTests> logger;

    private readonly IMatchingAggregationService svc;

    private readonly IMongoDbContext mongoContext;

    public GetImportNotificationsByCreatedDateTests(ITestOutputHelper testOutputHelper)
    {
        var builder = TestContextHelper.CreateBuilder<GetImportNotificationsByCreatedDateTests>(testOutputHelper);

        app = builder.Build();
        var rootScope = app.Services.CreateScope();
        logger = app.Services.GetRequiredService<ILogger<GetImportNotificationsByCreatedDateTests>>();
        svc = rootScope.ServiceProvider.GetRequiredService<IMatchingAggregationService>();
        mongoContext = rootScope.ServiceProvider.GetRequiredService<IMongoDbContext>();
    }

    [Fact]
    public async Task WhenCalled_ReturnExpectedAggregation()
    {
        await mongoContext.DropCollections();

        var scenario = app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 2);
        await app.PushToConsumers(scenario, 1);

        var noMatchScenario = app.CreateScenarioConfig<ChedANoMatchScenarioGenerator>(5, 2);
        await app.PushToConsumers(noMatchScenario, 2);

        var chedPScenario = app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 1);
        await app.PushToConsumers(chedPScenario, 3);

        var result = (await svc
            .GetImportNotificationLinkingByCreated())
            .ToList();

        logger.LogInformation(result.ToJsonString());

        result.Count.Should().Be(3);

        result[0].Name.Should().Be("Cveda Linked");
        result[0].Dates[0].Date.Should().BeOnOrBefore(DateOnly.FromDateTime(DateTime.Today));
        result[0].Dates.Count.Should().Be(30);
        
        result[1].Name.Should().Be("Cveda Not Linked");
        result[1].Dates[0].Date.Should().BeOnOrBefore(DateOnly.FromDateTime(DateTime.Today));
        result[1].Dates.Count.Should().Be(30);
        
        result[2].Name.Should().Be("Cvedp Linked");
        result[2].Dates[0].Date.Should().BeOnOrBefore(DateOnly.FromDateTime(DateTime.Today));
        result[2].Dates.Count.Should().Be(30);
    }
}