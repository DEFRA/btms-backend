using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class ItemsValidatorTests
{
    private ItemsValidator validator = new();

    [Fact]
    public void Should_have_error_when_ItemNumber_is_null()
    {
        var model = new Items { ItemNumber = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.ItemNumber);
    }

    [Fact]
    public void Should_not_have_error_when_ItemNumber_is_specified()
    {
        var model = new Items { ItemNumber = 1 };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.ItemNumber);
    }

    [Fact]
    public void Should_have_error_when_CustomsProcedureCode_is_null()
    {
        var model = new Items { CustomsProcedureCode = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.CustomsProcedureCode);
    }

    [Fact]
    public void Should_not_have_error_when_CustomsProcedureCode_is_specified()
    {
        var model = new Items { CustomsProcedureCode = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.CustomsProcedureCode);
    }

    [Fact]
    public void Should_have_error_when_TaricCommodityCode_is_null()
    {
        var model = new Items { TaricCommodityCode = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.TaricCommodityCode);
    }

    [Fact]
    public void Should_not_have_error_when_TaricCommodityCode_is_specified()
    {
        var model = new Items { TaricCommodityCode = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.TaricCommodityCode);
    }

    [Fact]
    public void Should_have_error_when_GoodsDescription_is_null()
    {
        var model = new Items { GoodsDescription = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.GoodsDescription);
    }

    [Fact]
    public void Should_not_have_error_when_GoodsDescription_is_specified()
    {
        var model = new Items { GoodsDescription = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.GoodsDescription);
    }

    [Fact]
    public void Should_have_error_when_ConsigneeId_is_null()
    {
        var model = new Items { ConsigneeId = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.ConsigneeId);
    }

    [Fact]
    public void Should_not_have_error_when_ConsigneeId_is_specified()
    {
        var model = new Items { ConsigneeId = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.ConsigneeId);
    }

    [Fact]
    public void Should_have_error_when_ConsigneeName_is_null()
    {
        var model = new Items { ConsigneeName = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.ConsigneeName);
    }

    [Fact]
    public void Should_not_have_error_when_ConsigneeName_is_specified()
    {
        var model = new Items { ConsigneeName = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.ConsigneeName);
    }

    [Fact]
    public void Should_have_error_when_ItemNetMass_is_null()
    {
        var model = new Items { ItemNetMass = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.ItemNetMass);
    }

    [Fact]
    public void Should_not_have_error_when_ItemNetMass_is_specified()
    {
        var model = new Items { ItemNetMass = 1 };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.ItemNetMass);
    }

    [Fact]
    public void Should_have_error_when_ItemOriginCountryCode_is_null()
    {
        var model = new Items { ItemOriginCountryCode = null };
        var result = validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(p => p.ItemOriginCountryCode);
    }

    [Fact]
    public void Should_not_have_error_when_ItemOriginCountryCode_is_specified()
    {
        var model = new Items { ItemOriginCountryCode = "test" };
        var result = validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(p => p.ItemOriginCountryCode);
    }


}