using Btms.Backend.Data.InMemory;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Types.Alvs;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TestDataGenerator;
using Xunit;

namespace Btms.Business.Tests.PreProcessing;

public class MovementPreProcessingTests
{
    [Fact]
    public async Task WhenNotificationNotExists_ThenShouldBeCreated()
    {
        // ARRANGE
        var clearanceRequest = CreateAlvsClearanceRequest();
        var dbContext = new MemoryMongoDbContext();
        var preProcessor = new MovementPreProcessor(dbContext, NullLogger<MovementPreProcessor>.Instance);
            

        // ACT
        var preProcessingResult = await preProcessor.Process(
            new PreProcessingContext<AlvsClearanceRequest>(clearanceRequest, "TestMessageId"));

        // ASSERT
        preProcessingResult.Outcome.Should().Be(PreProcessingOutcome.New);
        var savedMovement = await dbContext.Movements.Find(clearanceRequest.Header!.EntryReference!);
        savedMovement.Should().NotBeNull();
        savedMovement?.AuditEntries.Count.Should().Be(1);
        savedMovement?.AuditEntries[0].Status.Should().Be("Created");
    }

    private static AlvsClearanceRequest CreateAlvsClearanceRequest()
    {
        return ClearanceRequestBuilder.Default()
            .WithValidDocumentReferenceNumbers().Build();
    }
}