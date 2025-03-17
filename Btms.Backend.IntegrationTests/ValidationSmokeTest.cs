using Btms.Types.Alvs;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class ValidationSmokeTest(ITestOutputHelper output)
    : ScenarioGeneratorBaseTest<SingleCrWithFinalisationValidationErrorsScenarioGenerator>(output, backendConfigOverrides: new Dictionary<string, string>()
    {
        {"FeatureFlags:Validation_AlvsClearanceRequest", "true"},
        {"FeatureFlags:Validation_Finalisation", "true"},
    })
{
    [Fact]
    public void VerifyValidationErrorWrittenToDb()
    {
        BackendFixture.MongoDbContext.CdsValidationErrors.Count(x => x.Type == nameof(AlvsClearanceRequest)).Should().BeGreaterThan(0);
        BackendFixture.MongoDbContext.CdsValidationErrors.Count(x => x.Type == nameof(Finalisation)).Should().BeGreaterThan(0);
    }
}