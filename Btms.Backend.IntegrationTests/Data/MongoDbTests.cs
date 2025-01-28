using Btms.Backend.IntegrationTests.Helpers;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.Data;

[Trait("Category", "Integration")]
public class MongoDbTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<ApplicationFactory>
{
    private readonly ImportNotification _notification = new() { ReferenceNumber = "CHEDA.GB.2025.1111111" };
    
    [Fact]
    public async Task Insert_ShouldSetCreatedAndUpdated()
    {
        await ClearDb();
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SetUtcNow(now);
        
        await Factory.GetDbContext().Notifications.Insert(_notification);

        _notification.Created.Should().Be(now);
        _notification.Updated.Should().Be(now);
    }
    
    [Fact]
    public async Task Update_ShouldSetUpdated()
    {
        await ClearDb();
        var then = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SetUtcNow(then);
        await Factory.GetDbContext().Notifications.Insert(_notification);
        var now = then.AddDays(1);
        SetUtcNow(now);
        
        await Factory.GetDbContext().Notifications.Update(_notification);

        _notification.Created.Should().Be(then);
        _notification.Updated.Should().Be(now);
    }
    
    [Fact]
    public async Task Update_WhenSetUpdatedFalse_ShouldNotSetUpdated()
    {
        await ClearDb();
        var then = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SetUtcNow(then);
        await Factory.GetDbContext().Notifications.Insert(_notification);
        var now = then.AddDays(1);
        SetUtcNow(now);
        
        await Factory.GetDbContext().Notifications.Update(_notification, setUpdated: false);

        _notification.Created.Should().Be(then);
        _notification.Updated.Should().Be(then);
    }

    private void SetUtcNow(DateTime dateTime) =>
        ((FrozenTimeProvider)Factory.Services.GetRequiredService<TimeProvider>()).SetUtcNow(dateTime);
}