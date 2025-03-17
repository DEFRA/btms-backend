using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class ServiceHeaderValidatorTests
{
    private ServiceHeaderValidator validator = new();

    [Fact]
    public void Should_have_error_when_SourceSystem_is_null()
    {
        var model = new ServiceHeader { SourceSystem = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.SourceSystem);
    }

    [Fact]
    public void Should_not_have_error_when_SourceSystem_is_specified()
    {
        var model = new ServiceHeader { SourceSystem = "CDS" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.SourceSystem);
    }

    [Fact]
    public void Should_have_error_when_DestinationSystem_is_null()
    {
        var model = new ServiceHeader { CorrelationId = "123" };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.DestinationSystem);
    }

    [Fact]
    public void Should_not_have_error_when_DestinationSystem_is_specified()
    {
        var model = new ServiceHeader { DestinationSystem = "ALVS" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.DestinationSystem);
    }

    [Fact]
    public void Should_have_error_when_CorrelationId_is_null()
    {
        var model = new ServiceHeader { CorrelationId = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.CorrelationId);
    }

    [Fact]
    public void Should_not_have_error_when_CorrelationId_is_specified()
    {
        var model = new ServiceHeader { CorrelationId = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.CorrelationId);
    }

    [Fact]
    public void Should_have_error_when_ServiceCallTimestamp_is_null()
    {
        var model = new ServiceHeader { ServiceCallTimestamp = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.ServiceCallTimestamp);
    }

    [Fact]
    public void Should_not_have_error_when_ServiceCallTimestamp_is_specified()
    {
        var model = new ServiceHeader { ServiceCallTimestamp = DateTime.Now };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.ServiceCallTimestamp);
    }


}