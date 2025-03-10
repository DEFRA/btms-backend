using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class ItemsValidator : AbstractValidator<Items>
{
    public ItemsValidator()
    {
        RuleFor(p => p.ItemNumber).NotNull();
        RuleFor(p => p.CustomsProcedureCode).NotNull().NotEmpty();
        RuleFor(p => p.TaricCommodityCode).NotNull().NotEmpty();
        RuleFor(p => p.GoodsDescription).NotNull().NotEmpty();

        RuleFor(p => p.ConsigneeId).NotNull().NotEmpty();
        RuleFor(p => p.ConsigneeName).NotNull().NotEmpty();
        RuleFor(p => p.ItemNetMass).NotNull();
        RuleFor(p => p.ItemOriginCountryCode).NotNull();

        RuleForEach(p => p.Documents).SetValidator(new DocumentValidator());
        RuleForEach(p => p.Checks).SetValidator(new CheckValidator());
    }
}