using Btms.Backend.Data;
using Btms.Business.Services.Linking;
using Btms.Model.Ipaffs;
using NSubstitute;
using Xunit;

namespace Btms.Business.Tests.Services.Linking;

public class RelatedDataServiceTests
{
    private IMongoDbContext MongoDbContext { get; } = Substitute.For<IMongoDbContext>();
    
    [Fact]
    public async Task RelatedDataEntityChanged_Notification_ShouldBeUpdated()
    {
        var subject = new RelatedDataService(MongoDbContext);
        var notification = new ImportNotification();

        await subject.RelatedDataEntityChanged([notification], CancellationToken.None);

        await MongoDbContext.Notifications.Received().Update(notification, CancellationToken.None);
    }
}