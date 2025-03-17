using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class DocumentValidatorTests
{
    private DocumentValidator validator = new(1, "123");

    [Fact]
    public void Should_have_error_when_DocumentCode_is_null()
    {
        var model = new Document { DocumentCode = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.DocumentCode);
    }

    [Fact]
    public void Should_not_have_error_when_DocumentCode_is_specified()
    {
        var model = new Document { DocumentCode = "C633" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.DocumentCode);
    }

    [Fact]
    public void Should_have_error_when_DocumentStatus_is_null()
    {
        var model = new Document { DocumentStatus = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.DocumentStatus);
    }

    [Fact]
    public void Should_not_have_error_when_DocumentStatus_is_specified()
    {
        var model = new Document { DocumentStatus = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.DocumentStatus);
    }

    [Fact]
    public void Should_have_error_when_DocumentControl_is_null()
    {
        var model = new Document { DocumentControl = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.DocumentControl);
    }

    [Fact]
    public void Should_not_have_error_when_DocumentControl_is_specified()
    {
        var model = new Document { DocumentControl = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.DocumentControl);
    }
}