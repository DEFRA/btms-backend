using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class ItemsValidatorTests
{
    private readonly ItemsValidator validator = new("123");

    [Theory]
    [ClassData(typeof(ItemValidatorTestData))]
    public void TheoryTests(Items model, ExpectedResult expectedResult)
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

    public class ItemValidatorTestData : TheoryData<Items, ExpectedResult>
    {
        public ItemValidatorTestData()
        {
            Add(new Items { ItemNumber = 22 }, new ExpectedResult(nameof(Items.ItemNumber), false));
            Add(new Items { ItemNumber = 0 }, new ExpectedResult(nameof(Items.ItemNumber), true));
            Add(new Items { ItemNumber = 1000 }, new ExpectedResult(nameof(Items.ItemNumber), true));
            Add(new Items { ItemNumber = null }, new ExpectedResult(nameof(Items.ItemNumber), true));

            Add(new Items { CustomsProcedureCode = "Valid" }, new ExpectedResult(nameof(Items.CustomsProcedureCode), false));
            Add(new Items { CustomsProcedureCode = "fjrkdosp" }, new ExpectedResult(nameof(Items.CustomsProcedureCode), true));
            Add(new Items { CustomsProcedureCode = null }, new ExpectedResult(nameof(Items.CustomsProcedureCode), true));

            Add(new Items { TaricCommodityCode = "1234567899" }, new ExpectedResult(nameof(Items.TaricCommodityCode), false));
            Add(new Items { TaricCommodityCode = "0123456789" }, new ExpectedResult(nameof(Items.TaricCommodityCode), true));
            Add(new Items { TaricCommodityCode = "123456789" }, new ExpectedResult(nameof(Items.TaricCommodityCode), true));
            Add(new Items { TaricCommodityCode = null }, new ExpectedResult(nameof(Items.TaricCommodityCode), true));

            Add(new Items { GoodsDescription = "Valid" }, new ExpectedResult(nameof(Items.GoodsDescription), false));
            Add(new Items { GoodsDescription = null }, new ExpectedResult(nameof(Items.GoodsDescription), true));

            Add(new Items { ConsigneeId = "Valid" }, new ExpectedResult(nameof(Items.ConsigneeId), false));
            Add(new Items { ConsigneeId = "djgksospeksjdjdls;ldjfsl;dkflsdkfsdkfsdl;f" }, new ExpectedResult(nameof(Items.ConsigneeId), true));
            Add(new Items { ConsigneeId = null }, new ExpectedResult(nameof(Items.ConsigneeId), true));

            Add(new Items { ConsigneeName = "Valid" }, new ExpectedResult(nameof(Items.ConsigneeName), false));
            Add(new Items { ConsigneeName = "djgksospeksjdjdls;ldjfsl;dkflsdkfsdkfsdl;f" }, new ExpectedResult(nameof(Items.ConsigneeName), true));
            Add(new Items { ConsigneeName = null }, new ExpectedResult(nameof(Items.ConsigneeName), true));

            Add(new Items { ItemNetMass = (decimal?)16.5 }, new ExpectedResult(nameof(Items.ItemNetMass), false));
            Add(new Items { ItemNetMass = null }, new ExpectedResult(nameof(Items.ItemNetMass), true));

            Add(new Items { ItemOriginCountryCode = "GB" }, new ExpectedResult(nameof(Items.ItemOriginCountryCode), false));
            Add(new Items { ItemOriginCountryCode = "GB1" }, new ExpectedResult(nameof(Items.ItemOriginCountryCode), true));
            Add(new Items { ItemOriginCountryCode = null }, new ExpectedResult(nameof(Items.ItemOriginCountryCode), true));

        }
    }
}