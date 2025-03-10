using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class ItemsValidator : AbstractValidator<Items>
{
    public ItemsValidator()
    {
        RuleFor(p => p.ItemNumber).NotNull();
        RuleFor(p => p.CustomsProcedureCode).NotEmpty();
        RuleFor(p => p.TaricCommodityCode).NotEmpty();
        RuleFor(p => p.GoodsDescription).NotEmpty();

        RuleFor(p => p.ConsigneeId).NotEmpty();
        RuleFor(p => p.ConsigneeName).NotEmpty();
        RuleFor(p => p.ItemNetMass).NotNull();
        RuleFor(p => p.ItemOriginCountryCode).NotNull();

        RuleForEach(p => p.Documents).SetValidator(new DocumentValidator());
        RuleForEach(p => p.Checks).SetValidator(new CheckValidator());
    }
}