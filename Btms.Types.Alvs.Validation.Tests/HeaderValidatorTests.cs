using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class HeaderValidatorTests
{
    private HeaderValidator validator = new();

    [Fact]
    public void Should_have_error_when_EntryReference_is_null()
    {
        var model = new Header { EntryReference = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.EntryReference);
    }

    [Fact]
    public void Should_not_have_error_when_EntryReference_is_specified()
    {
        var model = new Header { EntryReference = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.EntryReference);
    }

    [Fact]
    public void Should_have_error_when_EntryVersionNumber_is_null()
    {
        var model = new Header { EntryVersionNumber = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.EntryVersionNumber);
    }

    [Fact]
    public void Should_not_have_error_when_EntryVersionNumber_is_specified()
    {
        var model = new Header { EntryVersionNumber = 1 };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.EntryVersionNumber);
    }

    [Fact]
    public void Should_have_error_when_DeclarationUcr_is_null()
    {
        var model = new Header { DeclarationUcr = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.DeclarationUcr);
    }

    [Fact]
    public void Should_not_have_error_when_DeclarationUcr_is_specified()
    {
        var model = new Header { DeclarationUcr = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.DeclarationUcr);
    }

    [Fact]
    public void Should_have_error_when_DeclarantId_is_null()
    {
        var model = new Header { DeclarantId = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.DeclarantId);
    }

    [Fact]
    public void Should_not_have_error_when_DeclarantId_is_specified()
    {
        var model = new Header { DeclarantId = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.DeclarantId);
    }

    [Fact]
    public void Should_have_error_when_DeclarantName_is_null()
    {
        var model = new Header { DeclarantName = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.DeclarantName);
    }

    [Fact]
    public void Should_not_have_error_when_DeclarantName_is_specified()
    {
        var model = new Header { DeclarantName = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.DeclarantName);
    }

    [Fact]
    public void Should_have_error_when_DispatchCountryCode_is_null()
    {
        var model = new Header { DispatchCountryCode = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.DispatchCountryCode);
    }

    [Fact]
    public void Should_not_have_error_when_DispatchCountryCode_is_specified()
    {
        var model = new Header { DispatchCountryCode = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.DispatchCountryCode);
    }


}