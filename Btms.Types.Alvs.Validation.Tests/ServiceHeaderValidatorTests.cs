using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class ServiceHeaderValidatorTests
{
    private ServiceHeaderValidator validator = new();

    [Theory]
    [ClassData(typeof(ServiceHeaderValidatorTestData))]
    public void TheoryTests(ServiceHeader model, ExpectedResult expectedResult)
    {
        var result = validator.TestValidate(model);

        if (expectedResult.HasValidationError)
        {
            result.ShouldHaveValidationErrorFor(expectedResult.PropertyName);
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(expectedResult.PropertyName);
        }
    }

    public class ServiceHeaderValidatorTestData : TheoryData<ServiceHeader, ExpectedResult>
    {
        public ServiceHeaderValidatorTestData()
        {
            Add(new ServiceHeader { SourceSystem = "CDS" }, new ExpectedResult(nameof(ServiceHeader.SourceSystem), false));
            Add(new ServiceHeader { SourceSystem = "SDC" }, new ExpectedResult(nameof(ServiceHeader.SourceSystem), true));
            Add(new ServiceHeader { SourceSystem = null }, new ExpectedResult(nameof(ServiceHeader.SourceSystem), true));

            Add(new ServiceHeader { DestinationSystem = "ALVS" }, new ExpectedResult(nameof(ServiceHeader.DestinationSystem), false));
            Add(new ServiceHeader { DestinationSystem = "SVLA" }, new ExpectedResult(nameof(ServiceHeader.DestinationSystem), true));
            Add(new ServiceHeader { DestinationSystem = null }, new ExpectedResult(nameof(ServiceHeader.DestinationSystem), true));

            Add(new ServiceHeader { CorrelationId = "test" }, new ExpectedResult(nameof(ServiceHeader.CorrelationId), false));
            Add(new ServiceHeader { CorrelationId = "123456789123456789123" }, new ExpectedResult(nameof(ServiceHeader.CorrelationId), true));
            Add(new ServiceHeader { CorrelationId = null }, new ExpectedResult(nameof(ServiceHeader.CorrelationId), true));

            Add(new ServiceHeader { ServiceCallTimestamp = DateTime.Now }, new ExpectedResult(nameof(ServiceHeader.ServiceCallTimestamp), false));
            Add(new ServiceHeader { ServiceCallTimestamp = null }, new ExpectedResult(nameof(ServiceHeader.ServiceCallTimestamp), true));
        }
    }
}