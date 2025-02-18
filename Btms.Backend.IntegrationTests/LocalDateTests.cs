using Btms.Backend.Data.Mongo;
using Btms.Common.Extensions;
using FluentAssertions;
using MongoDB.Bson;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class LocalDateTests(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<ChedASimpleMatchFixedDatesScenarioGenerator>(output)
{
    private static readonly DateTime expectedDateTime = new DateTime(2024, 12, 1, 10, 10, 0, DateTimeKind.Unspecified);

    [Fact]
    public async Task ShouldBeCorrectInApi()
    {
        var result = await Client.AsHttpClient().GetStringAsync(
            "api/import-notifications?fields[import-notifications]=partOne");

        var expectedDateString =
            $"\"departedOn\": \"{expectedDateTime:yyyy-MM-ddTHH:mm:ss}\"";

        result.Should().Contain(expectedDateString);
    }

    [Fact]
    public void ShouldBeCorrectInBtmsClient()
    {
        Client
            .GetSingleImportNotification()
            .PartOne!.DepartedOn
            .Should().Be(expectedDateTime).And.BeIn(DateTimeKind.Unspecified);
    }

    [Fact]
    public void ShouldBeCorrectInMongoDbContext()
    {
        BackendFixture.MongoDbContext.Notifications.Single()
            .PartOne!.DepartedOn
            .Should().Be(expectedDateTime).And.BeIn(DateTimeKind.Unspecified);
    }

    [Fact]
    public void ShouldBeAbleToQueryViaMongoDbContext()
    {
        BackendFixture.MongoDbContext.Notifications.Single(n => n.PartOne != null && n.PartOne!.DepartedOn == expectedDateTime)
            .PartOne!.DepartedOn
            .Should().Be(expectedDateTime).And.BeIn(DateTimeKind.Unspecified);
    }

    [Fact]
    public void ShouldBeCorrectInDatabase()
    {
        var command = @"{
            'find': 'ImportNotification',
            projection: {_id:0,'partOne.departedOn':1}
        }";

        var result = ((MongoDbContext)BackendFixture.MongoDbContext)
            .Database.RunCommand<BsonDocument>(command);

        result["cursor"]["firstBatch"][0]["partOne"]["departedOn"].AsBsonDateTime
            .Should().Be(new BsonDateTime(expectedDateTime));

        //Mongo stores things as UTC, not ideal but it is what it is!
        result["cursor"]["firstBatch"][0]["partOne"]["departedOn"].ToString()
            .Should().Be(expectedDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}