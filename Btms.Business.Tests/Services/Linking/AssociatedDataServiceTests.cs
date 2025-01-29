using Btms.Backend.Data.InMemory;
using Btms.Business.Services.Linking;
using Btms.Model.Auditing;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Services.Linking;

public class AssociatedDataServiceTests
{
    private string _existingId = nameof(_existingId);
    
    private readonly MemoryMongoDbContext _mongoDbContext = new();
    
    [Fact]
    public async Task RelatedDataEntityChanged_ExistingNotification_NoAuditEntries_UpdatesAndAddsFirstAuditEntry()
    {
        var subject = await CreateSubject();
        var existingNotification = await _mongoDbContext.Notifications.Find(x => x.Id == _existingId) ??
                                   throw new InvalidOperationException();

        await subject.RelatedDataEntityChanged([existingNotification], "audit-id", CancellationToken.None);

        _mongoDbContext.Notifications.Should()
            .ContainEquivalentOf(new
            {
                Id = _existingId,
                AuditEntries = (object[])
                [
                    new { Id = "audit-id", Status = "RelatedDataChanged", CreatedBy = CreatedBySystem.Btms, Version = 1 }
                ]
            });
    }
    
    [Fact]
    public async Task RelatedDataEntityChanged_ExistingNotification_ExistingAuditEntries_UpdatesAndAddsSecondAuditEntry()
    {
        var subject = await CreateSubject(addFirstAuditEntry: true);
        var existingNotification = await _mongoDbContext.Notifications.Find(x => x.Id == _existingId) ??
                                   throw new InvalidOperationException();

        await subject.RelatedDataEntityChanged([existingNotification], "audit-id", CancellationToken.None);

        _mongoDbContext.Notifications.Should()
            .ContainEquivalentOf(new
            {
                Id = _existingId,
                AuditEntries = (object[])
                [
                    new { Id = "first-audit-id", Version = 1 },
                    new { Id = "audit-id", Version = 1 }
                ]
            });
    }

    private async Task<AssociatedDataService> CreateSubject(bool addFirstAuditEntry = false)
    {
        var notification = new ImportNotification { Id = _existingId, Version = 1 };
        
        if (addFirstAuditEntry)
            notification.AuditEntries.Add(new AuditEntry
            {
                Id = "first-audit-id",
                Version = 1
            });
        
        await _mongoDbContext.Notifications.Insert(notification);
        
        return new AssociatedDataService(_mongoDbContext);
    }
}