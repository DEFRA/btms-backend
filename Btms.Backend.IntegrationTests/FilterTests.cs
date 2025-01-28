using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class FilterTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<ApplicationFactory>
{
    [Theory]
    [InlineData("X", "Expected format is two UTC dates separated by a comma, but was 'X'")]
    [InlineData("X,Y", "Expected DateTime, but was 'X'")]
    [InlineData("2025-01-28T10:53:00,Y", "Expected DateTime, but was 'Y'")]
    [InlineData("2025-01-28T10:53:00,2025-01-28T11:53:00", "Expected from date as UTC, but was '2025-01-28T10:53:00.0000000'")]
    [InlineData("2025-01-28T10:53:00Z,2025-01-28T11:53:00", "Expected to date as UTC, but was '2025-01-28T11:53:00.0000000'")]
    public void CustomFilter_RelationshipUpdated_Invalid_ShouldReturnError(string parameterValue, string expectedDetail)
    {
        var document = Client.AsJsonApiClient()
            .Get($"api/import-notifications?relationshipUpdated={parameterValue}", assertStatusCode: false);

        document.Errors.Should().ContainEquivalentOf(new
        {
            Status = "400",
            Title = "Invalid 'relationshipUpdated' query value",
            Detail = expectedDetail,
            Source = new { Parameter = "relationshipUpdated" }
        });
    }

    [Theory]
    [InlineData(14, 0, 0, true)]    // equal to lower bound
    [InlineData(14, 0, 1, true)]    // greater than lower bound
    [InlineData(14, 59, 59, true)]  // less than upper bound
    [InlineData(13, 59, 59, false)] // less than lower bound
    [InlineData(15, 0, 0, false)]   // equal to upper bound
    public async Task CustomFilter_RelationshipUpdated_Valid_ShouldReturnNotifications(int hour, int minute, int second, bool expectSingleNotification)
    {
        await ClearDb();
        var notification = new ImportNotification
        {
            Updated = new DateTime(2025, 1, 28, 12, 0, 0, DateTimeKind.Utc), _MatchReference = "123.456.789"
        };
        notification.AddRelationship(new TdmRelationshipObject
        {
            Data =
            [
                new RelationshipDataItem
                {
                    Type = "movements",
                    Id = "123456788",
                    Updated = new DateTime(2025, 1, 28, hour, minute, second, DateTimeKind.Utc)
                }
            ]
        });
        await Factory.GetDbContext().Notifications.Insert(notification);

        var document = Client.AsJsonApiClient()
            .Get("api/import-notifications?relationshipUpdated=2025-01-28T14:00:00Z,2025-01-28T15:00:00Z");

        document.Data.Count.Should().Be(expectSingleNotification ? 1 : 0);
    }
}