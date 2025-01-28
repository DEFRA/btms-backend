using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    public void CustomFilter_Updated_Invalid_ShouldReturnError(string parameterValue, string expectedDetail)
    {
        var document = Client.AsJsonApiClient()
            .Get($"api/import-notifications?updated={parameterValue}", assertStatusCode: false);

        document.Errors.Should().ContainEquivalentOf(new
        {
            Status = "400",
            Title = "Invalid 'updated' query value",
            Detail = expectedDetail,
            Source = new { Parameter = "updated" }
        });
    }

    [Theory]
    [InlineData(14, 0, 0, true)]    // equal to lower bound
    [InlineData(14, 0, 1, true)]    // greater than lower bound
    [InlineData(14, 59, 59, true)]  // less than upper bound
    [InlineData(13, 59, 59, false)] // less than lower bound
    [InlineData(15, 0, 0, false)]   // equal to upper bound
    public async Task CustomFilter_Updated_Valid_ShouldReturnNotifications(int hour, int minute, int second, bool expectNotifications)
    {
        // The intent of this test is to seed one notification with an Updated property value
        // outside the date range that is being queried, but that has a relationship Updated
        // property value within the date range. Then have another notification with
        // an Updated property value within the date range and no relationships. This will
        // then prove our expected behaviour of:
        //
        //  notification.Updated in range OR any relationship.Updated in range
        //
        await ClearDb();
        var updated = new DateTime(2025, 1, 28, hour, minute, second, DateTimeKind.Utc);
        var notification = new ImportNotification { ReferenceNumber = "CHEDA.GB.2025.1111111" };
        notification.AddRelationship(new TdmRelationshipObject
        {
            Data =
            [
                new RelationshipDataItem { Type = "movements", Id = "123456788", Updated = updated }
            ]
        });
        var timeProvider = (FrozenTimeProvider) Factory.Services.GetRequiredService<TimeProvider>();
        // Freeze time so Insert call will use value
        timeProvider.SetUtcNow(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        await Factory.GetDbContext().Notifications.Insert(notification);
        var notification2 = new ImportNotification { ReferenceNumber = "CHEDA.GB.2025.2222222" };
        timeProvider.SetUtcNow(updated);
        await Factory.GetDbContext().Notifications.Insert(notification2);

        var document = Client.AsJsonApiClient()
            .Get("api/import-notifications?updated=2025-01-28T14:00:00Z,2025-01-28T15:00:00Z");

        document.Data.Count.Should().Be(expectNotifications ? 2 : 0);
    }
}