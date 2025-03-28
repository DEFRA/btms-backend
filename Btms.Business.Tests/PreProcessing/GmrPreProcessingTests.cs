using Btms.Backend.Data.InMemory;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Model.Auditing;
using Btms.Model.Relationships;
using Btms.Types.Gvms;
using FluentAssertions;
using TestDataGenerator.Helpers;
using Xunit;

namespace Btms.Business.Tests.PreProcessing;

public class GmrPreProcessingTests
{
    private MemoryMongoDbContext MongoDbContext { get; } = new();
    private GmrPreProcessor Subject { get; }

    public GmrPreProcessingTests()
    {
        Subject = new GmrPreProcessor(MongoDbContext);
    }

    [Fact]
    public async Task Process_WhenGmrIdIsNull_ThenSkip()
    {
        var message = BuilderHelpers.GetGmrBuilder("asb-gmr")
            .With(x => x.GmrId, (string?)null)
            .ValidateAndBuild();

        var result = await Subject.Process(new PreProcessingContext<Gmr>(message, "messageId"), CancellationToken.None);

        result.Outcome.Should().Be(PreProcessingOutcome.Skipped);
        result.Record.GetLatestAuditEntry().Status.Should().BeNull();
    }

    [Fact]
    public async Task Process_WhenGmrDoesNotExist_ThenInserted()
    {
        var message = BuilderHelpers.GetGmrBuilder("asb-gmr").ValidateAndBuild();

        var result = await Subject.Process(new PreProcessingContext<Gmr>(message, "messageId"), CancellationToken.None);

        result.Outcome.Should().Be(PreProcessingOutcome.New);
        MongoDbContext.Gmrs.Count().Should().Be(1);
    }

    [Fact]
    public async Task Process_WhenGmrExists_ThenUpdated()
    {
        var created = new DateTime(2025, 2, 10, 12, 54, 0);
        var updated = new DateTime(2025, 2, 10, 12, 55, 0);
        var message = BuilderHelpers.GetGmrBuilder("asb-gmr")
            .With(x => x.UpdatedSource, updated.AddSeconds(1))
            .ValidateAndBuild();
        var gmr = new Model.Gvms.Gmr
        {
            Id = message.GmrId,
            UpdatedSource = updated,
            Created = created,
            AuditEntries =
            [
                new AuditEntry { Status = "Existing" }
            ],
            Relationships = new GmrRelationships
            {
                ImportNotifications = new TdmRelationshipObject { Data = [new RelationshipDataItem { Id = "123" }] }
            }
        };
        await MongoDbContext.Gmrs.Insert(gmr, CancellationToken.None);

        var result = await Subject.Process(new PreProcessingContext<Gmr>(message, "messageId"), CancellationToken.None);

        result.Outcome.Should().Be(PreProcessingOutcome.Changed);
        MongoDbContext.Gmrs.Count().Should().Be(1);
        MongoDbContext.Gmrs.First().Created.Should().Be(created);
        MongoDbContext.Gmrs.First().AuditEntries.Should().BeEquivalentTo([
            new { Status = "Existing" },
            new { Status = "Updated" },
        ]);
        MongoDbContext.Gmrs.First().Relationships.Should().BeEquivalentTo(gmr.Relationships);
    }

    [Fact]
    public async Task Process_WhenGmrExists_AndMessageHasNotChanged_ThenSkipped()
    {
        var updated = new DateTime(2025, 2, 10, 12, 55, 0);
        var message = BuilderHelpers.GetGmrBuilder("asb-gmr")
            .With(x => x.UpdatedSource, updated)
            .ValidateAndBuild();
        var existing = new Model.Gvms.Gmr { Id = message.GmrId, UpdatedSource = updated };
        await MongoDbContext.Gmrs.Insert(existing, CancellationToken.None);

        var result = await Subject.Process(new PreProcessingContext<Gmr>(message, "messageId"), CancellationToken.None);

        result.Outcome.Should().Be(PreProcessingOutcome.Skipped);
        result.Record.Should().Be(existing);
    }
}