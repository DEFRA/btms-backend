using Btms.Business.Services.Linking;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Linking;

public class UnLinkingTests : LinkingServiceTests
{

    [Fact]
    public async Task Unlink_Notification_And_Movements()
    {
        // Arrange
        var testData = await AddTestData(2, 3, 1);

        var sut = CreateLinkingService();
        await sut.Link(new ImportNotificationLinkContext(testData.Cheds[0], null));
        var notificationCtx = CreateNotificationContext(testData.Cheds[0], true, false);

        // Act
        await sut.UnLink(notificationCtx);

        var loadedNotification = await dbContext.Notifications.Find(testData.Cheds[0].Id!);
        loadedNotification?.Relationships.Movements.Data.Should().BeNullOrEmpty();

        var loadedMovements = dbContext.Movements.ToList();
        loadedMovements.ForEach(m => m.Relationships.Notifications.Data.Should().BeNullOrEmpty());
    }
}