using Btms.Backend.Data.InMemory;
using Btms.Business.Builders;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Model;
using Btms.Model.Validation;
using Btms.Types.Alvs;
using Btms.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TestDataGenerator;
using Xunit;

namespace Btms.Business.Tests.PreProcessing;

public class StubValidator : IBtmsValidator
{
    private readonly IEnumerable<BtmsValidationFailure> _failures;
    public StubValidator(IEnumerable<BtmsValidationFailure> failures)
    {
        _failures = failures;
    }

    public StubValidator()
    {
        _failures = [];
    }
    public BtmsValidationResult Validate<T>(T entity)
    {
        return new BtmsValidationResult(_failures);
    }
}

public class MovementPreProcessingTests
{
    [Fact]
    public async Task WhenNotificationNotExists_ThenShouldBeCreated()
    {
        // ARRANGE
        var clearanceRequest = CreateAlvsClearanceRequest();
        var dbContext = new MemoryMongoDbContext();
        var preProcessor = new MovementPreProcessor(dbContext, NullLogger<MovementPreProcessor>.Instance, new MovementBuilderFactory(new DecisionStatusFinder(), new BusinessDecisionStatusFinder(), NullLogger<MovementBuilder>.Instance), new StubValidator());

        // ACT
        var preProcessingResult = await preProcessor.Process(
            new PreProcessingContext<AlvsClearanceRequest>(clearanceRequest, "TestMessageId"));

        // ASSERT
        preProcessingResult.Outcome.Should().Be(PreProcessingOutcome.New);
        var savedMovement = await dbContext.Movements.Find(clearanceRequest.Header!.EntryReference!);
        savedMovement.Should().NotBeNull();
        savedMovement?.AuditEntries.Count.Should().Be(1);
        savedMovement?.AuditEntries[0].Status.Should().Be("Created");
        savedMovement?.Updated.Should().BeAfter(default);
    }

    [Fact]
    public async Task WhenClearenRequestFailsValidation_ThenShouldBeReturnValidationError()
    {
        // ARRANGE
        var clearanceRequest = CreateAlvsClearanceRequest();
        var dbContext = new MemoryMongoDbContext();
        var validator = new StubValidator([new BtmsValidationFailure("Test", "test", "test", null, ValidationSeverity.Error)]);
        var preProcessor = new MovementPreProcessor(dbContext, NullLogger<MovementPreProcessor>.Instance, new MovementBuilderFactory(new DecisionStatusFinder(), new BusinessDecisionStatusFinder(), NullLogger<MovementBuilder>.Instance), validator);

        // ACT
        var preProcessingResult = await preProcessor.Process(
            new PreProcessingContext<AlvsClearanceRequest>(clearanceRequest, "TestMessageId"));

        // ASSERT
        preProcessingResult.Outcome.Should().Be(PreProcessingOutcome.ValidationError);
        var savedMovement = await dbContext.Movements.Find(clearanceRequest.Header!.EntryReference!);
        savedMovement.Should().BeNull();
    }

    private static AlvsClearanceRequest CreateAlvsClearanceRequest()
    {
        return ClearanceRequestBuilder.Default()
            .WithValidDocumentReferenceNumbers().Build();
    }
}